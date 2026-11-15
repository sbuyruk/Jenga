using Jenga.DataAccess.Data;
using Jenga.DataAccess.Services.Common;
using Jenga.Models.Enums;
using Jenga.Models.IKYS;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using System.Net;

namespace Jenga.DataAccess.Services.IKYS;

public sealed class TaskApprovalEntryService(
    IDbContextFactory<ApplicationDbContext> dbFactory,
    ILogService logService,
    IGorevOnayService gorevOnayService,
    IServiceAuthorizationContext authorization,
    IEmailService? emailService = null,
    IConfiguration? configuration = null) : ITaskApprovalEntryService
{
    private static readonly CultureInfo Tr = new("tr-TR");
    private static readonly string[] Countries = ["Türkiye", "Avrupa", "İngiltere", "Avrupa Harici"];
    private static readonly string[] Transportation = ["", "Uçak", "Otobüs", "Tren", "Vakıf Aracı"];
    private static readonly string[] Transfers = ["", "Vakıf Aracı", "Toplu Taşıma", "Taksi", "Diğer"];
    private static readonly string[] Accommodation = ["Limit Dahilinde", "Limit Aşımı"];
    private int PersonnelUnitId => int.TryParse(configuration?["TaskApproval:PersonnelUnitId"], out var id) ? id : 5;
    private bool ApprovalEnabled => !bool.TryParse(configuration?["TaskApproval:ManagerApprovalEnabled"], out var enabled) || enabled;
    private static bool IsAuth(string? value, string expected) => string.Equals(value, expected, StringComparison.OrdinalIgnoreCase);

    public async Task<Result<TaskApprovalEntryLoadResult>> LoadAsync(TaskApprovalEntryLoadRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await authorization.HasPermissionAsync(ModuleName.IKYS, Operation.View, cancellationToken))
                return Result.Failure<TaskApprovalEntryLoadResult>(Error.Forbidden("Görev onay girişini görüntüleme yetkiniz bulunmuyor."));
            var organization = IsAuth(request.Auth, "IKYS");
            if (organization && !await authorization.HasPermissionAsync(ModuleName.IKYS, Operation.Manage, cancellationToken))
                return Result.Failure<TaskApprovalEntryLoadResult>(Error.Forbidden("Bu görünüm için İKYS yönetim yetkisi gereklidir."));
            await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
            var options = await LoadPeopleAsync(db, request.CurrentPersonelId, request.Auth, request.QueryBirimTanimId, cancellationToken);
            GorevOnay model;
            if (request.GorevOnayId.HasValue)
            {
                var entity = await db.GorevOnay_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.GorevOnayId, cancellationToken);
                if (entity is null || (!organization && (!options.Any(x => x.Id == entity.PersonelId) || !CanEditPersonal(entity))))
                    return Result.Failure<TaskApprovalEntryLoadResult>(Error.Forbidden("Görev düzenlenmeye uygun değil veya erişim yetkiniz bulunmuyor."));
                model = entity;
                if (organization && options.All(x => x.Id != model.PersonelId))
                {
                    var owner = await db.Personel_Table.AsNoTracking().Where(x => x.Id == model.PersonelId)
                        .Select(x => new TaskApprovalOptionItem { Id = x.Id, Label = x.Adi + " " + x.Soyadi }).FirstOrDefaultAsync(cancellationToken);
                    if (owner is not null) options.Add(owner);
                }
            }
            else
            {
                var selected = options.FirstOrDefault(x => x.Id == request.QueryPersonelId)?.Id
                    ?? options.FirstOrDefault(x => x.Id == request.CurrentPersonelId)?.Id ?? options.FirstOrDefault()?.Id;
                model = new GorevOnay
                {
                    PersonelId = selected, BaslangicTarihi = DateTime.Today.AddHours(8), BitisTarihi = DateTime.Today.AddHours(17),
                    Konaklama = Accommodation[0], UlasimAraci = "", Transfer = "", ParaBirimi = "TL", Odendi = false
                };
            }
            if (string.IsNullOrWhiteSpace(model.Konaklama)) model.Konaklama = Accommodation[0];
            var groupId = await FindGroupAsync(db, model.PersonelId, cancellationToken);
            var rates = await db.Harcirah_Table.AsNoTracking().Where(x => x.KadroGrupId == groupId).ToListAsync(cancellationToken);
            var country = rates.Where(x => x.ParaBirimi == model.ParaBirimi).OrderByDescending(x => x.BaslangicTarihi).Select(x => x.Ulke).FirstOrDefault() ?? Countries[0];
            var rate = rates.Where(x => x.Ulke == country && x.BaslangicTarihi.Date <= (model.BaslangicTarihi ?? DateTime.Today).Date)
                .OrderByDescending(x => x.BaslangicTarihi).FirstOrDefault();
            var directors = await LoadSignaturesAsync(db, true, cancellationToken);
            var specialists = await LoadSignaturesAsync(db, false, cancellationToken);
            if (model.Id == 0 && int.TryParse(configuration?["TaskApproval:DefaultSpecialistId"] ?? "124", out var specialist) && specialists.Any(x => x.Id == specialist))
                model.OnayImza = specialist;
            return Result.Success(new TaskApprovalEntryLoadResult
            {
                Model = model, IsEditMode = model.Id > 0, IsPaid = model.Odendi == true,
                ShowSave = model.Id == 0 && options.Count > 0,
                ShowUpdate = model.Id > 0 && (organization ? CanEditGeneral(model) : CanEditPersonal(model)),
                ShowDelete = model.Id > 0 && (organization ? CanEditGeneral(model) : CanEditPersonal(model)),
                ShowReport = organization && model.Id > 0 && model.Odendi != true && model.AmirOnayi is 1 or 3,
                PersonOptions = options, SelectedPersonDisplayName = options.FirstOrDefault(x => x.Id == model.PersonelId)?.Label,
                HarcirahGroup = rate?.KadroGrubu ?? "Tanımlanmamış", SelectedCountry = country,
                HarcirahHesaplansin = model.Id == 0 || ParseAmount(model.Yevmiye) != 0,
                PerDirOptions = directors, PerUzmOptions = specialists,
                UlasimAraclari = Transportation.ToList(), TransferSecenekleri = Transfers.ToList(), KonaklamaSecenekleri = Accommodation.ToList(),
                CountryRates = Countries.Select(x => new TaskApprovalCountryRateOption { Country = x }).ToList()
            });
        }
        catch (Exception ex)
        {
            logService.LogException(ex, "TaskApprovalEntry.LoadAsync");
            return Result.Failure<TaskApprovalEntryLoadResult>(Error.Unexpected("Görev onay giriş verileri hazırlanamadı.", ex));
        }
    }

    public async Task<TaskApprovalCalculationResult> CalculateAsync(TaskApprovalCalculationInput input, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!input.PersonelId.HasValue)
                return new() { ErrorMessage = "Personel seçimi zorunludur." };
            if (!await authorization.HasPermissionAsync(ModuleName.IKYS, Operation.View, cancellationToken))
                return new() { ErrorMessage = "Harcırah hesaplama yetkiniz bulunmuyor." };
            await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
            if (IsAuth(input.Auth, "IKYS"))
            {
                if (!await authorization.HasPermissionAsync(ModuleName.IKYS, Operation.Manage, cancellationToken))
                    return new() { ErrorMessage = "Bu personel için harcırah hesaplama yetkiniz bulunmuyor." };
            }
            else
            {
                var people = await LoadPeopleAsync(db, input.CurrentPersonelId, input.Auth, null, cancellationToken);
                if (!people.Any(x => x.Id == input.PersonelId))
                    return new() { ErrorMessage = "Seçilen personel işlem kapsamınızın dışında." };
            }
            return await CalculateCoreAsync(db, input, cancellationToken);
        }
        catch (Exception ex)
        {
            logService.LogException(ex, "TaskApprovalEntry.CalculateAsync");
            return new() { ErrorMessage = "Harcırah hesabı yapılamadı. Harcırah tanımlarını kontrol edin." };
        }
    }

    private async Task<TaskApprovalCalculationResult> CalculateCoreAsync(ApplicationDbContext db, TaskApprovalCalculationInput input, CancellationToken cancellationToken)
    {
        var group = await FindGroupAsync(db, input.PersonelId, cancellationToken);
        var rates = await db.Harcirah_Table.AsNoTracking().Where(x => x.KadroGrupId == group && x.Ulke == input.Country).ToListAsync(cancellationToken);
        var result = TaskApprovalAllowanceCalculator.Calculate(input, rates);
        if (!ApprovalEnabled) result.ApprovalStatus = ApprovalStatus.NotRequired;
        return result;
    }

    public Task<Result<bool>> HasOverlapAsync(int personelId, DateTime startDate, DateTime endDate, int? excludeTaskApprovalId = null, CancellationToken cancellationToken = default)
        => gorevOnayService.HasOverlappingTaskApprovalAsync(personelId, startDate, endDate, excludeTaskApprovalId, cancellationToken);

    public async Task<Result> DeleteAsync(int id, int? currentPersonelId, string? auth, CancellationToken cancellationToken = default)
    {
        try
        {
            var organization = IsAuth(auth, "IKYS");
            if (!await authorization.HasPermissionAsync(ModuleName.IKYS, organization ? Operation.Manage : Operation.Delete, cancellationToken))
                return Result.Failure(Error.Forbidden("Görev silme yetkiniz bulunmuyor."));
            await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
            await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
            var model = await db.GorevOnay_Table.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            var people = await LoadPeopleAsync(db, currentPersonelId, auth, null, cancellationToken);
            if (model is null || !(organization ? CanEditGeneral(model) : people.Any(x => x.Id == model.PersonelId) && CanEditPersonal(model)))
                return Result.Failure(Error.Forbidden("Görev silinmeye uygun değil veya erişim yetkiniz bulunmuyor."));
            db.SetCurrentUser(await authorization.GetUserNameAsync(cancellationToken));
            db.GorevOnay_Table.Remove(model);
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logService.LogException(ex, "TaskApprovalEntry.DeleteAsync");
            return Result.Failure(Error.Unexpected("Görev silinemedi.", ex));
        }
    }

    public async Task<Result<TaskApprovalEntrySaveResult>> SaveAsync(TaskApprovalEntrySaveRequest request, CancellationToken cancellationToken = default)
    {
        var model = request.Model;
        var organization = IsAuth(request.Auth, "IKYS");
        var isNew = model.Id == 0;
        try
        {
            if (!await authorization.HasPermissionAsync(ModuleName.IKYS, organization ? Operation.Manage : isNew ? Operation.Create : Operation.Edit, cancellationToken))
                return Result.Failure<TaskApprovalEntrySaveResult>(Error.Forbidden("Görev kaydetme yetkiniz bulunmuyor."));
            if (!model.PersonelId.HasValue || !model.BaslangicTarihi.HasValue || !model.BitisTarihi.HasValue || string.IsNullOrWhiteSpace(model.GorevinSebebi) || string.IsNullOrWhiteSpace(model.GorevinYeri))
                return Result.Failure<TaskApprovalEntrySaveResult>(Error.Validation("Personel, görev tarihleri, görevin sebebi ve yeri zorunludur."));
            await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
            await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
            var options = await LoadPeopleAsync(db, request.CurrentPersonelId, request.Auth, null, cancellationToken);
            var existing = isNew ? null : await db.GorevOnay_Table.FirstOrDefaultAsync(x => x.Id == model.Id, cancellationToken);
            if ((!isNew && (existing is null || existing.PersonelId != model.PersonelId || !(organization ? CanEditGeneral(existing) : CanEditPersonal(existing)))) ||
                ((!organization || isNew) && !options.Any(x => x.Id == model.PersonelId)))
                return Result.Failure<TaskApprovalEntrySaveResult>(Error.Forbidden("Görev kaydedilmeye uygun değil veya personel kapsamınızın dışında."));
            var calculation = await CalculateCoreAsync(db, new()
            {
                PersonelId = model.PersonelId, StartDate = model.BaslangicTarihi, EndDate = model.BitisTarihi,
                StartTime = model.BaslangicTarihi.Value.ToString("HH:mm"), EndTime = model.BitisTarihi.Value.ToString("HH:mm"),
                Country = request.Country, HarcirahHesaplansin = request.CalculateAllowance
            }, cancellationToken);
            if (!calculation.IsValid)
                return Result.Failure<TaskApprovalEntrySaveResult>(Error.Validation(calculation.ErrorMessage ?? "Harcırah hesabı geçersiz."));
            if (!Transportation.Contains(model.UlasimAraci ?? "") || !Transfers.Contains(model.Transfer ?? "") || !Accommodation.Contains(model.Konaklama ?? Accommodation[0]))
                return Result.Failure<TaskApprovalEntrySaveResult>(Error.Validation("Ulaşım, transfer veya konaklama seçimi geçersiz."));
            var directors = await LoadSignaturesAsync(db, true, cancellationToken);
            var specialists = await LoadSignaturesAsync(db, false, cancellationToken);
            if ((model.PerSubeImza is > 0 && !directors.Any(x => x.Id == model.PerSubeImza)) || (model.OnayImza is > 0 && !specialists.Any(x => x.Id == model.OnayImza)))
                return Result.Failure<TaskApprovalEntrySaveResult>(Error.Validation("İmzacı seçimi personel biriminin yetkili imzacıları arasından yapılmalıdır."));
            if (await db.GorevOnay_Table.AnyAsync(x => x.PersonelId == model.PersonelId && x.Id != model.Id && x.AmirOnayi != (int)ApprovalStatus.Rejected &&
                x.BaslangicTarihi < model.BitisTarihi && x.BitisTarihi > model.BaslangicTarihi, cancellationToken))
                return Result.Failure<TaskApprovalEntrySaveResult>(Error.Validation("Bu tarihlerde başka bir görev kaydı bulunmaktadır."));
            var entity = existing ?? new GorevOnay { PersonelId = model.PersonelId, Odendi = false, Secildi = false };
            entity.BaslangicTarihi = calculation.StartDateTime;
            entity.BitisTarihi = calculation.EndDateTime;
            entity.GorevinSebebi = model.GorevinSebebi.Trim();
            entity.GorevinYeri = model.GorevinYeri.Trim();
            entity.Aciklama = model.Aciklama;
            entity.AracPlakasi = model.AracPlakasi;
            entity.UlasimAraci = model.UlasimAraci ?? "";
            entity.AracTahsisi = model.UlasimAraci == "Uçak";
            entity.Transfer = model.Transfer ?? "";
            entity.Konaklama = model.Konaklama ?? Accommodation[0];
            entity.Avans = model.Avans;
            entity.PerSubeImza = model.PerSubeImza;
            entity.PerSubeVekil = model.PerSubeVekil;
            entity.OnayImza = model.OnayImza;
            entity.Sure = calculation.EarnedDayText;
            entity.Yevmiye = calculation.TotalAllowanceText;
            entity.GunlukYevmiye = calculation.HasMultipleRates ? "" : calculation.DailyAllowance.ToString("N2", Tr);
            entity.ParaBirimi = calculation.Currency;
            entity.AmirOnayi = (int)calculation.ApprovalStatus;
            entity.OnayRedAciklama = null;
            db.SetCurrentUser(await authorization.GetUserNameAsync(cancellationToken));
            if (isNew) db.GorevOnay_Table.Add(entity);
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            var warning = isNew ? await NotifyCreatedAsync(db, entity, organization, cancellationToken) : null;
            return Result.Success(new TaskApprovalEntrySaveResult { Id = entity.Id, Warning = warning, Calculation = calculation });
        }
        catch (Exception ex)
        {
            logService.LogException(ex, "TaskApprovalEntry.SaveAsync");
            return Result.Failure<TaskApprovalEntrySaveResult>(Error.Unexpected("Görev kaydedilemedi. Lütfen tekrar deneyin.", ex));
        }
    }

    private async Task<string?> NotifyCreatedAsync(ApplicationDbContext db, GorevOnay model, bool organization, CancellationToken cancellationToken)
    {
        const string warning = "Görev kaydedildi ancak bildirim e-postalarının tamamı gönderilemedi.";
        try
        {
            if (!ApprovalEnabled) return null;
            if (emailService is null) return warning;
            var person = await db.Personel_Table.AsNoTracking().FirstAsync(x => x.Id == model.PersonelId, cancellationToken);
            var ownEmail = await db.IletisimBilgileri_Table.AsNoTracking().Where(x => x.PersonelId == model.PersonelId).Select(x => x.InternetEPosta).FirstOrDefaultAsync(cancellationToken);
            var managerId = await (from job in db.IsBilgileri_Table.AsNoTracking()
                join unit in db.BirimTanim_Table.AsNoTracking() on job.BirimId equals unit.Id
                where job.PersonelId == model.PersonelId && job.CalismaDurumu == CalismaDurumu.Calisiyor select unit.AmirId).FirstOrDefaultAsync(cancellationToken);
            var managerEmail = await db.IletisimBilgileri_Table.AsNoTracking().Where(x => x.PersonelId == managerId).Select(x => x.InternetEPosta).FirstOrDefaultAsync(cancellationToken);
            var baseUrl = configuration?["TaskApproval:BaseUrl"]?.TrimEnd('/');
            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var applicationUri) || (applicationUri.Scheme != "http" && applicationUri.Scheme != "https")) baseUrl = null;
            // Recipients need their own routes even when HR created the task.
            const string personPath = "/ikys/task-approvals/personal";
            var managerPath = model.AmirOnayi == (int)ApprovalStatus.PendingApproval ? "/ikys/pending-approvals" : "";
            var subject = $"{person.Adi} {person.Soyadi} için {model.BaslangicTarihi:dd.MM.yyyy} - {model.BitisTarihi:dd.MM.yyyy} görev girişi yapıldı.";
            string Body(string path) => $"<p>{WebUtility.HtmlEncode(subject)}</p><p>{WebUtility.HtmlEncode(model.GorevinSebebi)} — {WebUtility.HtmlEncode(model.GorevinYeri)}</p>" +
                (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrEmpty(path) ? "" : $"<p><a href=\"{WebUtility.HtmlEncode(baseUrl + path + "?SecilenId=" + model.Id)}\">Görev işlemleri</a></p>");
            var delivered = true;
            if (!string.IsNullOrWhiteSpace(ownEmail)) delivered &= await emailService.TrySendAsync([ownEmail], subject, Body(personPath), cancellationToken);
            var recipients = new List<string>();
            if (!string.IsNullOrWhiteSpace(managerEmail)) recipients.Add(managerEmail);
            if (!organization)
            {
                var group = configuration?["TaskApproval:NotificationGroup"] ?? "gorevonaymailgrubu@tskgv.org.tr";
                recipients.AddRange(group.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));
            }
            if (recipients.Count > 0) delivered &= await emailService.TrySendAsync(recipients.Distinct(StringComparer.OrdinalIgnoreCase), subject, Body(managerPath), cancellationToken);
            return delivered && (!string.IsNullOrWhiteSpace(ownEmail) || recipients.Count > 0) ? null : warning;
        }
        catch (Exception ex)
        {
            logService.LogException(ex, "TaskApprovalEntry.NotifyCreatedAsync");
            return warning;
        }
    }

    private async Task<List<TaskApprovalOptionItem>> LoadPeopleAsync(ApplicationDbContext db, int? actorId, string? auth, int? requestedUnit, CancellationToken ct)
    {
        var organization = IsAuth(auth, "IKYS");
        if (organization && !await authorization.HasPermissionAsync(ModuleName.IKYS, Operation.Manage, ct)) return [];
        var query = from person in db.Personel_Table.AsNoTracking()
                    join job in db.IsBilgileri_Table.AsNoTracking() on person.Id equals job.PersonelId
                    where job.CalismaDurumu == CalismaDurumu.Calisiyor
                    select new { person.Id, person.Adi, person.Soyadi, person.Tipi, job.BirimId };
        if (organization) query = query.Where(x => x.Tipi == PersonelTipi.Kadrolu);
        else if (IsAuth(auth, "BIRIM"))
        {
            if (!actorId.HasValue) return [];
            var managed = await db.BirimTanim_Table.AsNoTracking().Where(x => x.AmirId == actorId && x.Aktif == true).Select(x => x.Id).ToListAsync(ct);
            var all = await db.BirimTanim_Table.AsNoTracking().Where(x => x.Aktif == true).Select(x => new { x.Id, x.ParentId }).ToListAsync(ct);
            var ids = managed.ToHashSet();
            for (var count = -1; count != ids.Count;)
            {
                count = ids.Count;
                foreach (var unit in all.Where(x => x.ParentId.HasValue && ids.Contains(x.ParentId.Value))) ids.Add(unit.Id);
            }
            if (requestedUnit.HasValue && !ids.Contains(requestedUnit.Value)) return [];
            if (requestedUnit.HasValue)
            {
                var subtree = new HashSet<int> { requestedUnit.Value };
                for (var count = -1; count != subtree.Count;)
                {
                    count = subtree.Count;
                    foreach (var unit in all.Where(x => x.ParentId.HasValue && subtree.Contains(x.ParentId.Value))) subtree.Add(unit.Id);
                }
                ids.IntersectWith(subtree);
            }
            query = query.Where(x => x.BirimId.HasValue && ids.Contains(x.BirimId.Value));
        }
        else
        {
            return await db.Personel_Table.AsNoTracking().Where(x => x.Id == actorId)
                .Select(x => new TaskApprovalOptionItem { Id = x.Id, Label = x.Adi + " " + x.Soyadi }).ToListAsync(ct);
        }
        return await query.Select(x => new TaskApprovalOptionItem { Id = x.Id, Label = x.Adi + " " + x.Soyadi }).Distinct().OrderBy(x => x.Label).ToListAsync(ct);
    }

    private async Task<List<TaskApprovalOptionItem>> LoadSignaturesAsync(ApplicationDbContext db, bool director, CancellationToken ct)
    {
        var candidates = await (from person in db.Personel_Table.AsNoTracking()
            join job in db.IsBilgileri_Table.AsNoTracking() on person.Id equals job.PersonelId
            join duty in db.GorevTanim_Table.AsNoTracking() on job.GorevId equals duty.Id
            where job.CalismaDurumu == CalismaDurumu.Calisiyor && job.BirimId == PersonnelUnitId && person.Tipi == PersonelTipi.Kadrolu
            select new { person.Id, Label = person.Adi + " " + person.Soyadi, duty.Adi }).ToListAsync(ct);
        return candidates.Where(x => director
                ? HasTitle(x.Adi, "Direktör", "Başuzman", "Kıdemli Uzman") : HasTitle(x.Adi, "Uzman", "Kıdemli Uzman"))
            .Select(x => new TaskApprovalOptionItem { Id = x.Id, Label = x.Label }).DistinctBy(x => x.Id).OrderBy(x => x.Label).ToList();
    }

    private static bool HasTitle(string? title, params string[] values) => values.Any(x => Tr.CompareInfo.IndexOf(title ?? "", x, CompareOptions.IgnoreCase) >= 0);
    private static bool CanEditPersonal(GorevOnay model) => model.Odendi == false && model.AmirOnayi is 0 or 3 && model.BitisTarihi >= DateTime.Now.AddDays(-14);
    private static bool CanEditGeneral(GorevOnay model) => model.Odendi != true && model.AmirOnayi is 0 or 1 or 3;
    private static decimal ParseAmount(string? value) => decimal.TryParse(value, NumberStyles.Number, Tr, out var amount) ? amount : 0;
    private static async Task<int> FindGroupAsync(ApplicationDbContext db, int? personelId, CancellationToken ct)
    {
        var direct = await db.GorevTanim_Table.AsNoTracking().Where(x => x.PersonelId == personelId && x.Aktif == true)
            .OrderBy(x => x.Id).Select(x => x.HarcirahGrupId).FirstOrDefaultAsync(ct);
        if (direct.HasValue) return direct.Value;
        return await (from job in db.IsBilgileri_Table.AsNoTracking()
            join duty in db.GorevTanim_Table.AsNoTracking() on job.GorevId equals duty.Id
            where job.PersonelId == personelId select duty.HarcirahGrupId).FirstOrDefaultAsync(ct) ?? 0;
    }
}
