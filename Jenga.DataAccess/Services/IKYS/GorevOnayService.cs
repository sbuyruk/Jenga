using Jenga.DataAccess.Data;
using Jenga.DataAccess.Services.Common;
using Jenga.Models.Enums;
using Jenga.Models.IKYS;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Globalization;
using System.Text;

namespace Jenga.DataAccess.Services.IKYS;

public class GorevOnayService : IGorevOnayService
{
    private const string Source = nameof(GorevOnayService);
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly ILogService _logService;
    private readonly IEmailService _emailService;
    private readonly IServiceAuthorizationContext _authorization;

    public GorevOnayService(
        IDbContextFactory<ApplicationDbContext> dbFactory,
        ILogService logService,
        IEmailService emailService,
        IServiceAuthorizationContext authorization)
    {
        _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _authorization = authorization ?? throw new ArgumentNullException(nameof(authorization));
    }

    public async Task<Result<List<GorevOnay>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var list = await db.GorevOnay_Table.AsNoTracking().ToListAsync(cancellationToken);
            return Result.Success(list);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.GetAllAsync");
            return Result.Failure<List<GorevOnay>>(Error.Unexpected("Görev onayları getirilemedi.", ex, "GorevOnay.GetAll.Failed"));
        }
    }

    public async Task<Result<List<GorevOnay>>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var list = await db.GorevOnay_Table.AsNoTracking().Where(x => x.PersonelId == personelId).ToListAsync(cancellationToken);
            return Result.Success(list);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.GetByPersonelIdAsync");
            return Result.Failure<List<GorevOnay>>(Error.Unexpected("Görev onayları getirilemedi.", ex, "GorevOnay.GetByPersonelId.Failed"));
        }
    }

    public async Task<Result<GorevOnay>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var entity = await db.GorevOnay_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (entity is null)
                return Result.Failure<GorevOnay>(Error.NotFound($"Görev onayı bulunamadı (Id={id}).", "GorevOnay.NotFound"));
            return Result.Success(entity);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.GetByIdAsync");
            return Result.Failure<GorevOnay>(Error.Unexpected("Görev onayı getirilemedi.", ex, "GorevOnay.GetById.Failed"));
        }
    }

    public async Task<Result> AddAsync(GorevOnay entity, string? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            return Result.Failure(Error.Validation("Görev onayı boş olamaz.", "GorevOnay.Null"));
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            db.SetCurrentUser(modifiedBy);
            await db.GorevOnay_Table.AddAsync(entity, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.AddAsync");
            return Result.Failure(Error.Unexpected("Görev onayı eklenemedi.", ex, "GorevOnay.Add.Failed"));
        }
    }

    public async Task<Result> UpdateAsync(GorevOnay entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            return Result.Failure(Error.Validation("Görev onayı boş olamaz.", "GorevOnay.Null"));
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var existing = await db.GorevOnay_Table.FirstOrDefaultAsync(x => x.Id == entity.Id, cancellationToken);
            if (existing is null)
                return Result.Failure(Error.NotFound("Kayıt bulunamadı!", "GorevOnay.NotFound"));
            existing.PersonelId = entity.PersonelId;
            existing.GorevinSebebi = entity.GorevinSebebi;
            existing.GorevinYeri = entity.GorevinYeri;
            existing.BaslangicTarihi = entity.BaslangicTarihi;
            existing.BitisTarihi = entity.BitisTarihi;
            existing.Sure = entity.Sure;
            existing.Avans = entity.Avans;
            existing.Yevmiye = entity.Yevmiye;
            existing.ParaBirimi = entity.ParaBirimi;
            existing.AracTahsisi = entity.AracTahsisi;
            existing.AracPlakasi = entity.AracPlakasi;
            existing.PerSubeImza = entity.PerSubeImza;
            existing.PerSubeVekil = entity.PerSubeVekil;
            existing.OnayImza = entity.OnayImza;
            existing.OnayMakam = entity.OnayMakam;
            existing.OnayMakamVekil = entity.OnayMakamVekil;
            existing.GMImza = entity.GMImza;
            existing.GMVekil = entity.GMVekil;
            existing.UlasimAraci = entity.UlasimAraci;
            existing.Secildi = entity.Secildi;
            existing.GunlukYevmiye = entity.GunlukYevmiye;
            existing.Odendi = entity.Odendi;
            existing.Aciklama = entity.Aciklama;
            existing.AmirOnayi = entity.AmirOnayi;
            existing.Transfer = entity.Transfer;
            existing.Konaklama = entity.Konaklama;
            existing.OnayRedAciklama = entity.OnayRedAciklama;
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.UpdateAsync");
            return Result.Failure(Error.Unexpected("Görev onayı güncellenemedi.", ex, "GorevOnay.Update.Failed"));
        }
    }

    public async Task<Result> DeleteAsync(GorevOnay entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            return Result.Failure(Error.Validation("Görev onayı boş olamaz.", "GorevOnay.Null"));
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            db.GorevOnay_Table.Attach(entity);
            db.GorevOnay_Table.Remove(entity);
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.DeleteAsync");
            return Result.Failure(Error.Unexpected("Görev onayı silinemedi.", ex, "GorevOnay.Delete.Failed"));
        }
    }

    /// <inheritdoc/>
    public async Task<Result<List<PendingApprovalItem>>> GetPendingByManagerAsync(
        int managerPersonelId, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

            // Find all units where the current user is the manager
            var managerUnitIds = await db.BirimTanim_Table
                .AsNoTracking()
                .Where(b => b.AmirId == managerPersonelId && b.Aktif == true)
                .Select(b => b.Id)
                .ToListAsync(cancellationToken);

            if (managerUnitIds.Count == 0)
                return Result.Success(new List<PendingApprovalItem>());

            // Fetch GorevOnay records for personnel assigned to those units
            var items = await (
                from go in db.GorevOnay_Table.AsNoTracking()
                join p in db.Personel_Table.AsNoTracking() on go.PersonelId equals p.Id
                join ib in db.IsBilgileri_Table.AsNoTracking() on p.Id equals ib.PersonelId
                where managerUnitIds.Contains(ib.BirimId ?? 0)
                    && (go.AmirOnayi == (int)ApprovalStatus.PendingApproval
                        || go.AmirOnayi == (int)ApprovalStatus.Approved
                        || go.AmirOnayi == (int)ApprovalStatus.Rejected)
                orderby go.BaslangicTarihi descending
                select new PendingApprovalItem
                {
                    GorevOnayId = go.Id,
                    PersonelId = go.PersonelId,
                    AdiSoyadi = p.Adi + " " + p.Soyadi,
                    BaslangicTarihi = go.BaslangicTarihi,
                    BitisTarihi = go.BitisTarihi,
                    Sure = go.Sure,
                    GorevinSebebi = go.GorevinSebebi,
                    GorevinYeri = go.GorevinYeri,
                    UlasimAraci = go.UlasimAraci,
                    Transfer = go.Transfer,
                    Konaklama = go.Konaklama,
                    Aciklama = go.Aciklama,
                    AmirOnayi = go.AmirOnayi ?? (int)ApprovalStatus.Other,
                    OnayRedAciklama = go.OnayRedAciklama
                }
            ).ToListAsync(cancellationToken);

            return Result.Success(items);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.GetPendingByManagerAsync");
            return Result.Failure<List<PendingApprovalItem>>(Error.Unexpected(
                "Bekleyen onaylar getirilemedi.", ex, "GorevOnay.GetPending.Failed"));
        }
    }

    /// <inheritdoc/>
    public async Task<Result> ApproveAsync(int gorevOnayId, int managerPersonelId, string? approvedBy = null, CancellationToken cancellationToken = default, bool authorizedIkysView = false)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            if (!await _authorization.HasPermissionAsync(ModuleName.IKYS, Operation.Manage, cancellationToken))
                return Result.Failure(Error.Forbidden("Bu işlem için yetkiniz bulunmuyor."));
            var managedPersonelIds = CreateManagedPersonelIdsQuery(db, managerPersonelId);
            var entity = await db.GorevOnay_Table
                .Include(g => g.Personel)
                .Where(x => x.Id == gorevOnayId && (authorizedIkysView || managedPersonelIds.Contains(x.PersonelId)))
                .AsTracking()
                .FirstOrDefaultAsync(cancellationToken);
            if (entity is null)
                return Result.Failure(Error.NotFound($"Görev onayı bulunamadı (Id={gorevOnayId}).", "GorevOnay.NotFound"));

            if (entity.AmirOnayi != (int)ApprovalStatus.PendingApproval)
                return Result.Failure(Error.Validation("Sadece bekleyen görevler onaylanabilir.", "GorevOnay.Approve.InvalidState"));

            entity.AmirOnayi = (int)ApprovalStatus.Approved;
            db.SetCurrentUser(approvedBy);
            entity.Degistiren = approvedBy;
            entity.DegistirmeTarihi = DateTime.Now;
            await db.SaveChangesAsync(cancellationToken);

            await SendApprovalEmailAsync(entity, db, "Onay", cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.ApproveAsync");
            return Result.Failure(Error.Unexpected("Onaylama işlemi başarısız oldu.", ex, "GorevOnay.Approve.Failed"));
        }
    }

    /// <inheritdoc/>
    public async Task<Result> RejectAsync(int gorevOnayId, int managerPersonelId, string rejectReason, string? rejectedBy = null, CancellationToken cancellationToken = default, bool authorizedIkysView = false)
    {
        if (string.IsNullOrWhiteSpace(rejectReason))
            return Result.Failure(Error.Validation("Red gerekçesi boş olamaz.", "GorevOnay.RejectReason.Empty"));

        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            if (!await _authorization.HasPermissionAsync(ModuleName.IKYS, Operation.Manage, cancellationToken))
                return Result.Failure(Error.Forbidden("Bu işlem için yetkiniz bulunmuyor."));
            var managedPersonelIds = CreateManagedPersonelIdsQuery(db, managerPersonelId);
            var entity = await db.GorevOnay_Table
                .Include(g => g.Personel)
                .Where(x => x.Id == gorevOnayId && (authorizedIkysView || managedPersonelIds.Contains(x.PersonelId)))
                .AsTracking()
                .FirstOrDefaultAsync(cancellationToken);
            if (entity is null)
                return Result.Failure(Error.NotFound($"Görev onayı bulunamadı (Id={gorevOnayId}).", "GorevOnay.NotFound"));

            if (entity.AmirOnayi != (int)ApprovalStatus.PendingApproval)
                return Result.Failure(Error.Validation("Sadece bekleyen görevler reddedilebilir.", "GorevOnay.Reject.InvalidState"));

            entity.AmirOnayi = (int)ApprovalStatus.Rejected;
            db.SetCurrentUser(rejectedBy);
            entity.OnayRedAciklama = rejectReason;
            entity.Degistiren = rejectedBy;
            entity.DegistirmeTarihi = DateTime.Now;
            await db.SaveChangesAsync(cancellationToken);

            await SendApprovalEmailAsync(entity, db, "Red", cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.RejectAsync");
            return Result.Failure(Error.Unexpected("Reddetme işlemi başarısız oldu.", ex, "GorevOnay.Reject.Failed"));
        }
    }

    private async Task<bool> CanReviewAsync(bool authorizedIkysView, CancellationToken cancellationToken)
        => await _authorization.HasPermissionAsync(ModuleName.IKYS, Operation.View, cancellationToken) &&
           (!authorizedIkysView || await _authorization.HasPermissionAsync(ModuleName.IKYS, Operation.Manage, cancellationToken));

    public async Task<Result<PendingApprovalsOverview>> GetPendingApprovalsAsync(
        int? currentPersonelId, bool authorizedIkysView, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await CanReviewAsync(authorizedIkysView, cancellationToken))
                return Result.Failure<PendingApprovalsOverview>(Error.Forbidden("Bu görünüm için yetkiniz bulunmuyor."));
            if (!authorizedIkysView && (!currentPersonelId.HasValue || currentPersonelId <= 0))
                return Result.Failure<PendingApprovalsOverview>(Error.Validation("Mevcut kullanıcı bilgisi alınamadı."));

            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var query = CreatePendingApprovalQuery(db);
            var ownTasks = authorizedIkysView ? query : query.Where(x => x.PersonelId == currentPersonelId);
            var pending = await OrderTasks(ownTasks.Where(x => x.AmirOnayi == (int)ApprovalStatus.PendingApproval))
                .ToListAsync(cancellationToken);
            var approved = await OrderTasks(ownTasks.Where(x => x.AmirOnayi == (int)ApprovalStatus.Approved))
                .Take(5).ToListAsync(cancellationToken);

            var hasManagerUnits = authorizedIkysView || await db.BirimTanim_Table.AsNoTracking()
                .AnyAsync(x => x.AmirId == currentPersonelId && x.Aktif == true, cancellationToken);
            var managedIds = CreateManagedPersonelIdsQuery(db, currentPersonelId ?? 0);
            var reviewTasks = authorizedIkysView ? query : query.Where(x => managedIds.Contains(x.PersonelId));
            var reviewItems = await reviewTasks.Where(x =>
                    x.AmirOnayi == (int)ApprovalStatus.PendingApproval ||
                    x.AmirOnayi == (int)ApprovalStatus.Approved ||
                    x.AmirOnayi == (int)ApprovalStatus.Rejected)
                .OrderBy(x => x.AmirOnayi == (int)ApprovalStatus.PendingApproval ? 0 : 1)
                .ThenByDescending(x => x.BaslangicTarihi).ThenByDescending(x => x.BitisTarihi)
                .ThenByDescending(x => x.GorevOnayId).ToListAsync(cancellationToken);

            return Result.Success(new PendingApprovalsOverview
            {
                HasManagerUnits = hasManagerUnits, ReviewItems = reviewItems,
                PendingTasks = pending, RecentApprovedTasks = approved
            });
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.GetPendingApprovalsAsync");
            return Result.Failure<PendingApprovalsOverview>(Error.Unexpected(
                "Bekleyen işlemler getirilemedi.", ex, "GorevOnay.Overview.Failed"));
        }
    }

    private static IOrderedQueryable<PendingApprovalItem> OrderTasks(IQueryable<PendingApprovalItem> query)
        => query.OrderByDescending(x => x.BaslangicTarihi).ThenByDescending(x => x.BitisTarihi)
            .ThenByDescending(x => x.GorevOnayId);

    private static IQueryable<PendingApprovalItem> CreatePendingApprovalQuery(ApplicationDbContext db)
        => from go in db.GorevOnay_Table.AsNoTracking()
           join p in db.Personel_Table.AsNoTracking() on go.PersonelId equals p.Id
           select new PendingApprovalItem
           {
               GorevOnayId = go.Id, PersonelId = go.PersonelId,
               AdiSoyadi = (p.Adi ?? "") + " " + (p.Soyadi ?? ""),
               BaslangicTarihi = go.BaslangicTarihi, BitisTarihi = go.BitisTarihi,
               Sure = go.Sure, GorevinSebebi = go.GorevinSebebi, GorevinYeri = go.GorevinYeri,
               UlasimAraci = go.UlasimAraci, Transfer = go.Transfer, Konaklama = go.Konaklama,
               Aciklama = go.Aciklama, AmirOnayi = go.AmirOnayi ?? (int)ApprovalStatus.Other, OnayRedAciklama = go.OnayRedAciklama
           };

    public async Task<Result<List<TaskApprovalListItem>>> GetTaskApprovalListAsync(
        int? currentPersonelId,
        bool authorizedUnitView,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await HasTaskPermissionAsync(authorizedUnitView, Operation.View, cancellationToken))
                return Result.Failure<List<TaskApprovalListItem>>(Error.Forbidden("Bu listeyi görüntüleme yetkiniz bulunmuyor."));
            if (!authorizedUnitView && (!currentPersonelId.HasValue || currentPersonelId <= 0))
                return Result.Failure<List<TaskApprovalListItem>>(Error.Validation("Mevcut kullanıcı personel kaydıyla eşleşmedi."));

            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var query = CreateTaskApprovalQuery(db);
            if (authorizedUnitView)
                query = query.Where(x => x.EndDate >= DateTime.Today.AddYears(-1));
            else
                query = query.Where(x => x.PersonelId == currentPersonelId);

            var items = authorizedUnitView
                ? await query.OrderByDescending(x => x.StartDate)
                    .ThenBy(x => x.ManagerApprovalValue == (int)ApprovalStatus.PendingApproval ? 0 : 1)
                    .ThenByDescending(x => x.EndDate).ThenByDescending(x => x.TaskApprovalId).ToListAsync(cancellationToken)
                : await query.OrderByDescending(x => x.EndDate).ThenByDescending(x => x.StartDate)
                    .ThenByDescending(x => x.TaskApprovalId).ToListAsync(cancellationToken);

            ApplyCalculationFlags(items);
            return Result.Success(items);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.GetTaskApprovalListAsync");
            return Result.Failure<List<TaskApprovalListItem>>(Error.Unexpected(
                "Görev onay listesi getirilemedi.", ex, "GorevOnay.TaskApprovalList.Failed"));
        }
    }

    public async Task<Result<List<TaskApprovalListItem>>> GetTaskApprovalReportItemsAsync(
        IReadOnlyCollection<int> gorevOnayIds,
        int? currentPersonelId,
        bool authorizedUnitView,
        CancellationToken cancellationToken = default)
    {
        if (gorevOnayIds is null || gorevOnayIds.Count == 0)
            return Result.Success(new List<TaskApprovalListItem>());

        try
        {
            if (!authorizedUnitView || !await HasTaskPermissionAsync(true, Operation.View, cancellationToken))
                return Result.Failure<List<TaskApprovalListItem>>(Error.Forbidden("Görev raporu almak için İKYS yönetim yetkisi gereklidir."));
            var requestedIds = gorevOnayIds.Distinct().ToArray();
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var items = await CreateTaskApprovalQuery(db)
                .Where(x => requestedIds.Contains(x.TaskApprovalId) && !x.IsPaid &&
                    (x.ManagerApprovalValue == (int)ApprovalStatus.Approved || x.ManagerApprovalValue == (int)ApprovalStatus.NotRequired))
                .OrderBy(x => x.StartDate).ToListAsync(cancellationToken);
            if (items.Count != requestedIds.Length)
                return Result.Failure<List<TaskApprovalListItem>>(Error.Validation("Seçilen görevlerden biri rapor almaya uygun değil. Listeyi yenileyip tekrar seçin."));

            await PopulateReportDetailsAsync(db, items, cancellationToken);
            ApplyCalculationFlags(items);
            return Result.Success(items);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.GetTaskApprovalReportItemsAsync");
            return Result.Failure<List<TaskApprovalListItem>>(Error.Unexpected(
                "Görev onay raporu verileri getirilemedi.", ex, "GorevOnay.TaskApprovalReport.Failed"));
        }
    }

    private static async Task PopulateReportDetailsAsync(ApplicationDbContext db, List<TaskApprovalListItem> items, CancellationToken ct)
    {
        var ids = items.Select(x => x.TaskApprovalId).ToArray();
        var records = await db.GorevOnay_Table.AsNoTracking().Where(x => ids.Contains(x.Id))
            .Select(x => new { x.Id, x.PersonelId, x.Avans, x.AracPlakasi, x.AracTahsisi, x.OnayImza, x.PerSubeImza, x.PerSubeVekil })
            .ToDictionaryAsync(x => x.Id, ct);
        var units = await db.BirimTanim_Table.AsNoTracking().Where(x => x.Aktif == true && x.AmirId.HasValue)
            .Select(x => new { x.Adi, x.KisaAdi, x.AmirId }).ToListAsync(ct);
        var financeManagerIds = units.Where(x => IsFinanceDirectorate(x.Adi) || IsFinanceDirectorate(x.KisaAdi))
            .Select(x => x.AmirId!.Value).Distinct().ToArray();
        var activeManagers = await db.IsBilgileri_Table.AsNoTracking()
            .Where(x => financeManagerIds.Contains(x.PersonelId) && x.CalismaDurumu == CalismaDurumu.Calisiyor)
            .Select(x => x.PersonelId).Distinct().ToListAsync(ct);
        int? financeId = activeManagers.Count == 1 ? activeManagers[0] : null;
        var peopleIds = records.Values.SelectMany(x => new[] { x.PersonelId, x.OnayImza, x.PerSubeImza }).Append(financeId)
            .Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToArray();
        var names = await db.Personel_Table.AsNoTracking().Where(x => peopleIds.Contains(x.Id))
            .Select(x => new { x.Id, Name = (x.Adi ?? "") + " " + (x.Soyadi ?? "") }).ToDictionaryAsync(x => x.Id, x => x.Name.Trim(), ct);
        var jobs = await (from job in db.IsBilgileri_Table.AsNoTracking()
                          join duty in db.GorevTanim_Table.AsNoTracking() on job.GorevId equals duty.Id into duties
                          from duty in duties.DefaultIfEmpty()
                          join title in db.UnvanTanim_Table.AsNoTracking() on job.UnvanId equals title.Id into titles
                          from title in titles.DefaultIfEmpty()
                          where peopleIds.Contains(job.PersonelId)
                          select new { job.Id, job.PersonelId, job.CalismaDurumu, DutyShort = duty.KisaAdi, Duty = duty.Adi, TitleShort = title.KisaAdi, Title = title.Adi }).ToListAsync(ct);
        var dutyNames = jobs.GroupBy(x => x.PersonelId).ToDictionary(x => x.Key, x =>
        {
            var job = x.OrderByDescending(j => j.CalismaDurumu == CalismaDurumu.Calisiyor).ThenByDescending(j => j.Id).First();
            return new[] { job.DutyShort, job.Duty, job.TitleShort, job.Title }.FirstOrDefault(n => !string.IsNullOrWhiteSpace(n)) ?? "";
        });
        TaskApprovalReportSignature Signature(int? id, bool deputy = false) => new(id,
            id.HasValue ? names.GetValueOrDefault(id.Value, "") : "",
            id.HasValue ? dutyNames.GetValueOrDefault(id.Value, "") : "", deputy);
        foreach (var item in items)
        {
            var record = records[item.TaskApprovalId];
            item.DutyTitle = record.PersonelId.HasValue ? dutyNames.GetValueOrDefault(record.PersonelId.Value, "") : "";
            item.AdvanceText = record.Avans;
            item.VehiclePlate = record.AracPlakasi;
            item.VehicleRequested = record.AracTahsisi;
            item.SpecialistSignature = Signature(record.OnayImza);
            item.PersonnelDirectorSignature = Signature(record.PerSubeImza, record.PerSubeVekil == true);
            var finance = Signature(financeId);
            item.FinanceDirectorSignature = finance with { Title = "Muh. ve Fin. Dir." };
        }
    }

    private static bool IsFinanceDirectorate(string? name)
    {
        var upper = (name ?? "").ToUpperInvariant().Replace('İ', 'I').Replace('Ü', 'U').Replace('Ö', 'O').Replace('Ş', 'S').Replace('Ğ', 'G').Replace('Ç', 'C');
        var normalized = new string(upper.Where(char.IsLetterOrDigit).ToArray());
        return normalized.Contains("MUHASEBE") && normalized.Contains("FINANS") && normalized.Contains("DIREKTOR")
            || normalized is "MUHVEFINDIR" or "MUHVEFINDIREKTORLUGU";
    }

    public async Task<Result<GorevOnay>> GetScopedByIdAsync(
        int id,
        int? currentPersonelId,
        bool authorizedUnitView,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await HasTaskPermissionAsync(authorizedUnitView, Operation.Edit, cancellationToken))
                return Result.Failure<GorevOnay>(Error.Forbidden("Bu görevi düzenleme yetkiniz bulunmuyor."));
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var entity = await EditableTaskApprovals(db, currentPersonelId, authorizedUnitView)
                .AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (entity is null)
                return Result.Failure<GorevOnay>(Error.NotFound("Görev onayı bulunamadı.", "GorevOnay.NotFound"));

            return Result.Success(entity);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.GetScopedByIdAsync");
            return Result.Failure<GorevOnay>(Error.Unexpected(
                "Görev onayı getirilemedi.", ex, "GorevOnay.GetScopedById.Failed"));
        }
    }

    public async Task<Result<List<Personel>>> GetManagedPersonnelAsync(
        int managerPersonelId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var managedPersonelIds = CreateManagedPersonelIdsQuery(db, managerPersonelId);
            var personnel = await db.Personel_Table.AsNoTracking()
                .Where(x => managedPersonelIds.Contains(x.Id))
                .OrderBy(x => x.Adi)
                .ThenBy(x => x.Soyadi)
                .ToListAsync(cancellationToken);

            return Result.Success(personnel);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.GetManagedPersonnelAsync");
            return Result.Failure<List<Personel>>(Error.Unexpected(
                "Yönetilen personel listesi getirilemedi.", ex, "GorevOnay.GetManagedPersonnel.Failed"));
        }
    }

    public async Task<Result> AddScopedAsync(
        GorevOnay entity,
        int? currentPersonelId,
        bool authorizedUnitView,
        string? modifiedBy = null,
        CancellationToken cancellationToken = default)
    {
        if (entity is null)
            return Result.Failure(Error.Validation("Görev onayı boş olamaz.", "GorevOnay.Null"));

        try
        {
            if (!await HasTaskPermissionAsync(authorizedUnitView, Operation.Create, cancellationToken))
                return Result.Failure(Error.Forbidden("Görev oluşturma yetkiniz bulunmuyor."));
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var validationError = await ValidateScopedWriteAsync(db, entity.PersonelId, currentPersonelId, authorizedUnitView, cancellationToken);
            if (validationError is not null)
                return Result.Failure(validationError);

            db.SetCurrentUser(modifiedBy);
            entity.Odendi = false;
            entity.Secildi = false;
            entity.AmirOnayi = (int)ApprovalStatus.PendingApproval;
            entity.OnayRedAciklama = null;
            await db.GorevOnay_Table.AddAsync(entity, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.AddScopedAsync");
            return Result.Failure(Error.Unexpected("Görev onayı eklenemedi.", ex, "GorevOnay.AddScoped.Failed"));
        }
    }

    public async Task<Result> UpdateScopedAsync(
        GorevOnay entity,
        int? currentPersonelId,
        bool authorizedUnitView,
        CancellationToken cancellationToken = default)
    {
        if (entity is null)
            return Result.Failure(Error.Validation("Görev onayı boş olamaz.", "GorevOnay.Null"));

        try
        {
            if (!await HasTaskPermissionAsync(authorizedUnitView, Operation.Edit, cancellationToken))
                return Result.Failure(Error.Forbidden("Bu görevi düzenleme yetkiniz bulunmuyor."));
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
            var existing = await EditableTaskApprovals(db, currentPersonelId, authorizedUnitView)
                .Where(x => x.Id == entity.Id).AsTracking().FirstOrDefaultAsync(cancellationToken);
            if (existing is null)
                return Result.Failure(Error.NotFound("Kayıt bulunamadı!", "GorevOnay.NotFound"));

            var validationError = await ValidateScopedWriteAsync(db, entity.PersonelId, currentPersonelId, authorizedUnitView, cancellationToken);
            if (validationError is not null)
                return Result.Failure(validationError);

            existing.PersonelId = entity.PersonelId;
            existing.GorevinSebebi = entity.GorevinSebebi;
            existing.GorevinYeri = entity.GorevinYeri;
            existing.BaslangicTarihi = entity.BaslangicTarihi;
            existing.BitisTarihi = entity.BitisTarihi;
            existing.Sure = entity.Sure;
            existing.Avans = entity.Avans;
            existing.Yevmiye = entity.Yevmiye;
            existing.ParaBirimi = entity.ParaBirimi;
            existing.AracTahsisi = entity.AracTahsisi;
            existing.AracPlakasi = entity.AracPlakasi;
            existing.PerSubeImza = entity.PerSubeImza;
            existing.PerSubeVekil = entity.PerSubeVekil;
            existing.OnayImza = entity.OnayImza;
            existing.OnayMakam = entity.OnayMakam;
            existing.OnayMakamVekil = entity.OnayMakamVekil;
            existing.GMImza = entity.GMImza;
            existing.GMVekil = entity.GMVekil;
            existing.UlasimAraci = entity.UlasimAraci;
            existing.GunlukYevmiye = entity.GunlukYevmiye;
            existing.Aciklama = entity.Aciklama;
            existing.AmirOnayi = authorizedUnitView ? entity.AmirOnayi : (int)ApprovalStatus.PendingApproval;
            existing.Transfer = entity.Transfer;
            existing.Konaklama = entity.Konaklama;
            existing.OnayRedAciklama = entity.OnayRedAciklama;
            db.SetCurrentUser(await _authorization.GetUserNameAsync(cancellationToken));
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.UpdateScopedAsync");
            return Result.Failure(Error.Unexpected("Görev onayı güncellenemedi.", ex, "GorevOnay.UpdateScoped.Failed"));
        }
    }

    private Task<bool> HasTaskPermissionAsync(bool organizationView, Operation personalOperation, CancellationToken cancellationToken)
        => _authorization.HasPermissionAsync(ModuleName.IKYS, organizationView ? Operation.Manage : personalOperation, cancellationToken);

    private static IQueryable<GorevOnay> EditableTaskApprovals(ApplicationDbContext db, int? personelId, bool organizationView)
    {
        var query = db.GorevOnay_Table.Where(x => x.Odendi != true);
        if (organizationView)
            return query.Where(x => x.AmirOnayi == (int)ApprovalStatus.PendingApproval ||
                x.AmirOnayi == (int)ApprovalStatus.Approved || x.AmirOnayi == (int)ApprovalStatus.NotRequired);
        var threshold = DateTime.Now.AddDays(-14);
        return query.Where(x => x.PersonelId == personelId && personelId.HasValue && x.Odendi == false &&
            (x.AmirOnayi == (int)ApprovalStatus.PendingApproval || x.AmirOnayi == (int)ApprovalStatus.NotRequired) &&
            x.BitisTarihi.HasValue && x.BitisTarihi >= threshold);
    }

    public async Task<Result> DeleteScopedAsync(int id, int? currentPersonelId, bool authorizedUnitView, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await HasTaskPermissionAsync(authorizedUnitView, Operation.Delete, cancellationToken))
                return Result.Failure(Error.Forbidden("Görev silme yetkiniz bulunmuyor."));
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
            var entity = await EditableTaskApprovals(db, currentPersonelId, authorizedUnitView)
                .Where(x => x.Id == id).AsTracking().FirstOrDefaultAsync(cancellationToken);
            if (entity is null)
                return Result.Failure(Error.Validation("Görev silinmeye uygun değil veya erişim yetkiniz bulunmuyor."));
            db.SetCurrentUser(await _authorization.GetUserNameAsync(cancellationToken));
            db.GorevOnay_Table.Remove(entity);
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.DeleteScopedAsync");
            return Result.Failure(Error.Unexpected("Görev silinemedi.", ex));
        }
    }

    public async Task<Result> SetTaskApprovalsPaidAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await HasTaskPermissionAsync(true, Operation.Manage, cancellationToken))
                return Result.Failure(Error.Forbidden("Ödendi işlemi için İKYS yönetim yetkisi gereklidir."));
            var selectedIds = ids.Distinct().ToArray();
            if (selectedIds.Length == 0)
                return Result.Failure(Error.Validation("Ödendi yapmak için en az bir görev seçin."));
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
            var entities = await db.GorevOnay_Table.Where(x => selectedIds.Contains(x.Id)).ToListAsync(cancellationToken);
            if (entities.Count != selectedIds.Length || entities.Any(x => x.Odendi == true ||
                (x.AmirOnayi != (int)ApprovalStatus.Approved && x.AmirOnayi != (int)ApprovalStatus.NotRequired)))
                return Result.Failure(Error.Validation("Seçilen görevlerden biri ödendi işlemine uygun değil. Listeyi yenileyip tekrar seçin."));
            db.SetCurrentUser(await _authorization.GetUserNameAsync(cancellationToken));
            foreach (var entity in entities) entity.Odendi = true;
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.SetTaskApprovalsPaidAsync");
            return Result.Failure(Error.Unexpected("Ödendi işlemi tamamlanamadı.", ex));
        }
    }

    public async Task<Result<bool>> HasOverlappingTaskApprovalAsync(
        int personelId,
        DateTime startDate,
        DateTime endDate,
        int? excludeTaskApprovalId = null,
        CancellationToken cancellationToken = default)
    {
        if (endDate <= startDate)
            return Result.Success(false);

        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var query = db.GorevOnay_Table.AsNoTracking()
                .Where(x => x.PersonelId == personelId && x.BaslangicTarihi.HasValue && x.BitisTarihi.HasValue);

            if (excludeTaskApprovalId.HasValue)
                query = query.Where(x => x.Id != excludeTaskApprovalId.Value);

            var hasOverlap = await query.AnyAsync(x =>
                x.BaslangicTarihi!.Value < endDate &&
                x.BitisTarihi!.Value > startDate, cancellationToken);

            return Result.Success(hasOverlap);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.HasOverlappingTaskApprovalAsync");
            return Result.Failure<bool>(Error.Unexpected(
                "Görev tarih çakışma kontrolü yapılamadı.", ex, "GorevOnay.OverlapCheck.Failed"));
        }
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private static readonly CultureInfo TrCulture = new("tr-TR");

    private static void ApplyCalculationFlags(List<TaskApprovalListItem> items)
    {
        foreach (var item in items)
        {
            if (item.StartDate.HasValue && DateTime.Today - item.StartDate.Value < TimeSpan.FromDays(32))
                item.HasCalculationError = HasCalculationError(item);
        }
    }

    private static bool HasCalculationError(TaskApprovalListItem item)
    {
        if (!item.StartDate.HasValue || !item.EndDate.HasValue)
            return false;

        var dailyAllowance = ParseDecimal(item.DailyAllowanceText, removeThousands: false);
        if (dailyAllowance < 1m)
            return false;

        var expectedDuration = CalculateDuration(item.StartDate.Value, item.EndDate.Value);
        var duration = ParseDecimal(item.DurationText, removeThousands: false);
        if (duration != expectedDuration)
            return true;

        var allowance = ParseDecimal(item.AllowanceText, removeThousands: true);
        var expectedAllowance = expectedDuration * dailyAllowance;
        return allowance != expectedAllowance;
    }

    private static decimal CalculateDuration(DateTime startDate, DateTime endDate)
    {
        if (endDate <= startDate)
            return 0m;

        var minutes = (endDate - startDate).TotalMinutes;
        var hours = (minutes / 60d) % 24d;
        var days = (int)((minutes / 60d) / 24d);
        var remainder = hours == 0d ? 0m : hours > 12d ? 1m : 0.5m;
        return days + remainder;
    }

    private static decimal ParseDecimal(string? value, bool removeThousands)
    {
        if (string.IsNullOrWhiteSpace(value))
            return 0m;

        var normalized = value.Trim()
            .Replace("gün", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("gun", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("TL", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("₺", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Trim();

        if (removeThousands)
            normalized = normalized.Replace(".", string.Empty, StringComparison.Ordinal);

        return decimal.TryParse(normalized, NumberStyles.Number, TrCulture, out var parsed)
            ? parsed
            : 0m;
    }

    private static IQueryable<TaskApprovalListItem> CreateTaskApprovalQuery(ApplicationDbContext db)
        => from go in db.GorevOnay_Table.AsNoTracking()
           join p in db.Personel_Table.AsNoTracking() on go.PersonelId equals p.Id into personelJoin
           from p in personelJoin.DefaultIfEmpty()
           select new TaskApprovalListItem
           {
               TaskApprovalId = go.Id,
               PersonelId = go.PersonelId,
               FullName = p == null
                   ? string.Empty
                   : ((p.Adi ?? string.Empty) + " " + (p.Soyadi ?? string.Empty)).Trim(),
               Reason = go.GorevinSebebi,
               Destination = go.GorevinYeri,
               StartDate = go.BaslangicTarihi,
               EndDate = go.BitisTarihi,
               DurationText = go.Sure,
               AllowanceText = go.Yevmiye,
               DailyAllowanceText = go.GunlukYevmiye,
               Currency = go.ParaBirimi,
               Transportation = go.UlasimAraci,
               Transfer = go.Transfer,
               Accommodation = go.Konaklama,
               Description = go.Aciklama,
               RejectionNote = go.OnayRedAciklama,
               IsSelected = go.Secildi ?? false,
               IsPaid = go.Odendi ?? false,
               PaymentStatus = go.Odendi,
               ManagerApprovalValue = go.AmirOnayi ?? (int)ApprovalStatus.Other
           };

    internal static IQueryable<GorevOnay> SearchableApprovals(ApplicationDbContext db, int personelId, bool canManage)
    {
        var since = DateTime.Today.AddYears(-1);
        var managedIds = CreateManagedPersonelIdsQuery(db, personelId);
        return db.GorevOnay_Table.AsNoTracking().Where(x => x.BitisTarihi >= since &&
            (x.PersonelId == personelId || (canManage && managedIds.Contains(x.PersonelId) &&
                x.AmirOnayi != (int)ApprovalStatus.Rejected)));
    }

    private static IQueryable<int?> CreateManagedPersonelIdsQuery(ApplicationDbContext db, int managerPersonelId)
        => from isBilgileri in db.IsBilgileri_Table.AsNoTracking()
           join birim in db.BirimTanim_Table.AsNoTracking() on isBilgileri.BirimId equals birim.Id
           where birim.AmirId == managerPersonelId && birim.Aktif == true
           select (int?)isBilgileri.PersonelId;

    private static async Task<Error?> ValidateScopedWriteAsync(
        ApplicationDbContext db,
        int? entityPersonelId,
        int? currentPersonelId,
        bool authorizedUnitView,
        CancellationToken cancellationToken)
    {
        if (!entityPersonelId.HasValue)
            return Error.Validation("Personel bilgisi zorunludur.", "GorevOnay.Personel.Required");
        if (!authorizedUnitView && entityPersonelId != currentPersonelId)
            return Error.Forbidden("Bu personel için işlem yetkiniz yok.");
        return await db.Personel_Table.AnyAsync(x => x.Id == entityPersonelId, cancellationToken)
            ? null : Error.Validation("Personel bulunamadı.");
    }

    private async Task SendApprovalEmailAsync(
        GorevOnay gorevOnay,
        ApplicationDbContext db,
        string durum,
        CancellationToken cancellationToken)
    {
        try
        {
            if (gorevOnay.PersonelId is null) return;

            var personel = gorevOnay.Personel
                ?? await db.Personel_Table.AsNoTracking()
                           .FirstOrDefaultAsync(p => p.Id == gorevOnay.PersonelId, cancellationToken);
            if (personel is null) return;

            var recipients = new List<string>();

            // Employee's own e-mail
            var iletisim = await db.IletisimBilgileri_Table.AsNoTracking()
                .FirstOrDefaultAsync(i => i.PersonelId == gorevOnay.PersonelId, cancellationToken);
            if (!string.IsNullOrWhiteSpace(iletisim?.InternetEPosta))
                recipients.Add(iletisim.InternetEPosta);

            // Manager's e-mail (via IsBilgileri → BirimTanim → AmirId)
            var isBilgileri = await db.IsBilgileri_Table.AsNoTracking()
                .FirstOrDefaultAsync(i => i.PersonelId == gorevOnay.PersonelId, cancellationToken);
            if (isBilgileri?.BirimId != null)
            {
                var birim = await db.BirimTanim_Table.AsNoTracking()
                    .FirstOrDefaultAsync(b => b.Id == isBilgileri.BirimId, cancellationToken);
                if (birim?.AmirId != null)
                {
                    var amirIletisim = await db.IletisimBilgileri_Table.AsNoTracking()
                        .FirstOrDefaultAsync(i => i.PersonelId == birim.AmirId, cancellationToken);
                    if (!string.IsNullOrWhiteSpace(amirIletisim?.InternetEPosta))
                        recipients.Add(amirIletisim.InternetEPosta);
                }
            }

            if (recipients.Count == 0) return;

            var durumStr = durum == "Onay" ? "onaylanmıştır" : "reddedilmiştir";
            var tarihAraligi = $"{gorevOnay.BaslangicTarihi:dd.MM.yyyy} - {gorevOnay.BitisTarihi:dd.MM.yyyy}";
            var adiSoyadi = $"{personel.Adi} {personel.Soyadi}";

            var subject = $"{adiSoyadi} için {tarihAraligi} tarihleri arasındaki görev {durumStr}.";
            var body = BuildEmailBody(gorevOnay, adiSoyadi, tarihAraligi, durumStr);

            await _emailService.SendAsync(recipients, subject, body, cancellationToken);
        }
        catch (Exception ex)
        {
            // E-mail failure must not roll back the DB transaction
            _logService.LogException(ex, $"{Source}.SendApprovalEmailAsync");
        }
    }

    private static string BuildEmailBody(GorevOnay gorevOnay, string adiSoyadi, string tarihAraligi, string durumStr)
    {
        var sb = new StringBuilder();
        sb.Append("<style>table{border-collapse:collapse;font-family:Arial,sans-serif;font-size:13px;width:60%} ");
        sb.Append("td,th{border:1px solid #ccc;padding:6px 10px;text-align:left} th{background:#f0f0f0}</style>");
        sb.Append($"<p>{WebUtility.HtmlEncode(adiSoyadi)} için {WebUtility.HtmlEncode(tarihAraligi)} tarihleri arasındaki görev <strong>{WebUtility.HtmlEncode(durumStr)}</strong>.</p>");
        sb.Append("<table>");
        sb.Append($"<tr><th colspan='2' style='text-align:center;background:#ddd'>Görev Bilgileri</th></tr>");
        AppendRow(sb, "Adı Soyadı", adiSoyadi);
        AppendRow(sb, "Tarih Aralığı", tarihAraligi);
        AppendRow(sb, "Süre", gorevOnay.Sure);
        AppendRow(sb, "Görevin Sebebi", gorevOnay.GorevinSebebi);
        AppendRow(sb, "Görevin Yeri", gorevOnay.GorevinYeri);
        AppendRow(sb, "Ulaşım Aracı", gorevOnay.UlasimAraci);
        AppendRow(sb, "Transfer", gorevOnay.Transfer);
        AppendRow(sb, "Konaklama", gorevOnay.Konaklama);
        AppendRow(sb, "Açıklama", gorevOnay.Aciklama);
        if (!string.IsNullOrWhiteSpace(gorevOnay.OnayRedAciklama))
            AppendRow(sb, "Onay/Red Açıklaması", gorevOnay.OnayRedAciklama);
        sb.Append("</table>");
        return sb.ToString();
    }

    private static void AppendRow(StringBuilder sb, string label, string? value)
        => sb.Append($"<tr><td><strong>{WebUtility.HtmlEncode(label)}</strong></td><td>{WebUtility.HtmlEncode(value ?? "-")}</td></tr>");
}
