using System.Data;
using System.Text.Json;
using Jenga.DataAccess.Data;
using Jenga.DataAccess.Services.Common;
using Jenga.Models.Enums;
using Jenga.Models.IKYS;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.IKYS;

public sealed class OvertimeService(IDbContextFactory<ApplicationDbContext> factory,
    IServiceAuthorizationContext authorization, ILogService log) : IOvertimeService
{
    private sealed record Actor(int Id, string UserName, string FullName, bool IsHr, List<int> ManagedUnits, List<int> EscalatedUnits);
    private sealed class RuleException(Error error) : Exception(error.Message)
    {
        public Error Error { get; } = error;
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new RuleException(Error.Validation(message, "Overtime.Validation"));
    }

    private static void Hr(Actor actor)
    {
        if (!actor.IsHr) throw new RuleException(Error.Forbidden("Bu işlem için mevcut İKYS yönetim yetkisi gereklidir."));
    }

    private async Task<Result<T>> Run<T>(bool write, Func<ApplicationDbContext, Actor, Task<T>> action, CancellationToken ct)
    {
        try
        {
            var userName = await authorization.GetUserNameAsync(ct);
            Require(!string.IsNullOrWhiteSpace(userName), "Oturum bilgisi bulunamadı.");
            var login = userName!.Split('\\').Last().Split('@')[0];
            await using var db = await factory.CreateDbContextAsync(ct);
            var person = await db.Personel_Table.AsNoTracking().SingleOrDefaultAsync(x => x.KullaniciAdi == login, ct);
            Require(person is not null, "Oturumunuz bir personel kaydıyla eşleştirilemedi.");
            var organization = await db.BirimTanim_Table.AsNoTracking().Where(x => x.Aktif == true).ToListAsync(ct);
            var units = organization.Where(x => x.AmirId == person!.Id).Select(x => x.Id).ToList();
            var escalated = organization.Where(unit => unit.AmirId.HasValue && unit.AmirId != person!.Id &&
                SuperiorManager(unit, organization) == person!.Id).Select(x => x.Id).ToList();
            var actor = new Actor(person!.Id, userName!, $"{person.Adi} {person.Soyadi}",
                await authorization.HasPermissionAsync(ModuleName.IKYS, Operation.Manage, ct), units, escalated);
            db.SetCurrentUser(actor.UserName);
            // A submission and a unit freeze must not commit against different sets of entries.
            await using var transaction = write ? await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct) : null;
            var value = await action(db, actor);
            if (write)
            {
                await db.SaveChangesAsync(ct);
                await transaction!.CommitAsync(ct);
            }
            return Result.Success(value);
        }
        catch (RuleException ex) { return Result.Failure<T>(ex.Error); }
        catch (DbUpdateConcurrencyException) { return Result.Failure<T>(Error.Conflict("Kayıt başka bir işlemde değişti. Ekranı yenileyip tekrar deneyin.")); }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            log.LogException(ex, nameof(OvertimeService));
            return Result.Failure<T>(Error.Unexpected("Fazla mesai işlemi tamamlanamadı. Kurulumun yapıldığını kontrol edin; ekranı yenileyip tekrar deneyin.", ex));
        }
    }

    private static void CheckVersion(byte[] current, byte[] expected)
        => Require(current.SequenceEqual(expected), "Kayıt değişti. Ekranı yenileyip tekrar deneyin.");

    private static int? SuperiorManager(BirimTanim unit, List<BirimTanim> organization)
    {
        var visited = new HashSet<int> { unit.Id };
        var parentId = unit.ParentId;
        while (parentId.HasValue && visited.Add(parentId.Value))
        {
            var parent = organization.SingleOrDefault(x => x.Id == parentId.Value);
            if (parent is null) break;
            if (parent.AmirId.HasValue && parent.AmirId != unit.AmirId) return parent.AmirId;
            parentId = parent.ParentId;
        }
        return null;
    }

    private static IQueryable<OvertimeEntry> Reviewable(ApplicationDbContext db, Actor actor)
        => db.OvertimeEntries.Where(x => x.PersonelId != actor.Id &&
            (actor.ManagedUnits.Contains(x.UnitId) || (actor.EscalatedUnits.Contains(x.UnitId) &&
                db.BirimTanim_Table.Any(u => u.Id == x.UnitId && u.AmirId == x.PersonelId))));

    private static IQueryable<int> WorkingPersonIds(ApplicationDbContext db)
        => db.IsBilgileri_Table.Where(x => x.CalismaDurumu == CalismaDurumu.Calisiyor).Select(x => x.PersonelId).Distinct();

    private static IQueryable<int> DelegatedPersonIds(ApplicationDbContext db, Actor actor)
        => db.OvertimeDelegations.Where(x => x.DelegatePersonelId == actor.Id && x.IsActive
            && WorkingPersonIds(db).Contains(x.DelegatePersonelId) && WorkingPersonIds(db).Contains(x.TargetPersonelId))
            .Select(x => x.TargetPersonelId);

    private static IQueryable<OvertimePersonOption> PersonOptions(ApplicationDbContext db, IQueryable<int>? personelIds = null)
    {
        var people = db.Personel_Table.AsNoTracking().Where(x => !string.IsNullOrWhiteSpace(x.Adi) && !string.IsNullOrWhiteSpace(x.Soyadi)
            && WorkingPersonIds(db).Contains(x.Id));
        if (personelIds is not null) people = people.Where(x => personelIds.Contains(x.Id));
        return people.OrderBy(x => x.Adi).ThenBy(x => x.Soyadi)
            .Select(x => new OvertimePersonOption(x.Id, x.Adi + " " + x.Soyadi, x.KullaniciAdi ?? "", x.SicilNo));
    }

    private static async Task RequireEntryAccess(ApplicationDbContext db, Actor actor, int personelId, CancellationToken ct)
    {
        if (personelId != actor.Id && !await DelegatedPersonIds(db, actor).ContainsAsync(personelId, ct))
            throw new RuleException(Error.Forbidden("Bu personel adına mesai girişi yapma yetkiniz bulunmuyor. İK ile görüşün."));
    }

    public Task<Result<OvertimeDelegationAdminData>> GetDelegationsAsync(CancellationToken cancellationToken = default)
        => Run(false, async (db, actor) =>
        {
            Hr(actor);
            return new OvertimeDelegationAdminData
            {
                People = await PersonOptions(db).ToListAsync(cancellationToken),
                Assignments = await db.OvertimeDelegations.AsNoTracking().Where(x => x.IsActive)
                    .Select(x => new OvertimeDelegationLink(x.DelegatePersonelId, x.TargetPersonelId)).ToListAsync(cancellationToken)
            };
        }, cancellationToken);

    public Task<Result<bool>> SetDelegationsAsync(int delegatePersonelId, List<int> targetPersonelIds, List<int> expectedTargetPersonelIds, CancellationToken cancellationToken = default)
        => Run(true, async (db, actor) =>
        {
            Hr(actor);
            var targets = targetPersonelIds.Distinct().OrderBy(x => x).ToList();
            Require(!targets.Contains(delegatePersonelId), "Kullanıcı kendi mesaisini zaten girebilir. Yetki listesinde başka personelleri seçin.");
            var user = await db.Personel_Table.AsNoTracking().Where(x => x.Id == delegatePersonelId)
                .Select(x => new { x.Id, x.KullaniciAdi }).SingleOrDefaultAsync(cancellationToken);
            Require(user is not null && (targets.Count == 0 || !string.IsNullOrWhiteSpace(user.KullaniciAdi)), "Mesai girişi yapacak kullanıcı için geçerli bir kullanıcı hesabı seçin.");
            Require(targets.Count == 0 || await WorkingPersonIds(db).ContainsAsync(delegatePersonelId, cancellationToken), "Yetki verilecek kullanıcının çalışma durumu Çalışıyor (1) olmalıdır.");
            Require(await WorkingPersonIds(db).CountAsync(id => targets.Contains(id), cancellationToken) == targets.Count,
                "Yalnızca çalışma durumu Çalışıyor (1) olan personeller için yetki tanımlanabilir. Listeyi yenileyin.");
            var assignments = await db.OvertimeDelegations.Where(x => x.DelegatePersonelId == delegatePersonelId).ToListAsync(cancellationToken);
            var current = assignments.Where(x => x.IsActive).Select(x => x.TargetPersonelId).OrderBy(x => x).ToList();
            if (!current.SequenceEqual(expectedTargetPersonelIds.Distinct().OrderBy(x => x)))
                throw new RuleException(Error.Conflict("Bu kullanıcının yetkileri başka bir işlemde değişti. Listeyi yenileyip tekrar deneyin."));
            if (current.SequenceEqual(targets)) return true;
            foreach (var assignment in assignments) assignment.IsActive = targets.Contains(assignment.TargetPersonelId);
            foreach (var target in targets.Where(id => assignments.All(x => x.TargetPersonelId != id)))
                db.OvertimeDelegations.Add(new() { DelegatePersonelId = delegatePersonelId, TargetPersonelId = target, IsActive = true });
            Audit(db, "Mesai giriş yetkileri güncellendi", $"Kullanıcı: {user!.KullaniciAdi} (personel {delegatePersonelId}). Önceki personel listesi: {string.Join(", ", current)}. Yeni personel listesi: {string.Join(", ", targets)}.");
            return true;
        }, cancellationToken);

    private static void Audit(ApplicationDbContext db, string action, string details, int? entryId = null, int? periodId = null)
        => db.OvertimeEvents.Add(new() { EntryId = entryId, PeriodId = periodId, Action = action, Details = details });

    private static IQueryable<OvertimePeriod> PeriodHeaders(ApplicationDbContext db) => db.OvertimePeriods.Select(x => new OvertimePeriod
    {
        Id = x.Id, Name = x.Name, StartDate = x.StartDate, EndDate = x.EndDate, DueDate = x.DueDate,
        Status = x.Status, SignatureRevision = x.SignatureRevision, Version = x.Version, SignedFileName = x.SignedFileName, FinalizedAt = x.FinalizedAt
    });

    public Task<Result<bool>> SetExternalInstitutionAsync(int personelId, string institution, CancellationToken cancellationToken = default)
        => Run(true, async (db, actor) =>
        {
            Hr(actor);
            var name = PersonnelInstitution.Normalize(institution);
            Require(name.Length is > 0 and <= 200, "Kadro harici personel için kurum adı zorunludur ve en fazla 200 karakter olabilir.");
            var info = await db.IsBilgileri_Table.Include(x => x.Personel).SingleOrDefaultAsync(x => x.PersonelId == personelId, cancellationToken);
            Require(info?.Personel.Tipi == PersonelTipi.KadroHarici, "Kurum bilgisi yalnızca kadro harici personel için tanımlanabilir.");
            var names = await db.IsBilgileri_Table.Where(x => x.KadroHariciKurum != null).Select(x => x.KadroHariciKurum).Distinct().ToListAsync(cancellationToken);
            info!.KadroHariciKurum = names.Select(PersonnelInstitution.Normalize).FirstOrDefault(x => PersonnelInstitution.Key(x) == PersonnelInstitution.Key(name)) ?? name;
            Audit(db, "Kadro harici personel kurumu güncellendi", $"Personel {personelId}; Kurum: {info.KadroHariciKurum}. Önceden girilmiş mesai kayıtlarının kurumu korunur.");
            return true;
        }, cancellationToken);

    public Task<Result<OvertimeWorkspace>> LoadAsync(string scope, DateTime start, DateTime end, CancellationToken cancellationToken = default)
        => Run(false, async (db, actor) =>
        {
            Require(end.Date >= start.Date && (end - start).TotalDays <= 370, "En fazla bir yıllık tarih aralığı seçin.");
            Require(scope is "mine" or "delegated" or "team" or "hr", "Geçersiz görünüm.");
            if (scope == "hr") Hr(actor);
            var entries = db.OvertimeEntries.AsNoTracking().Include(x => x.Intervals)
                .Where(x => x.WorkDate >= start.Date && x.WorkDate <= end.Date);
            if (scope == "mine") entries = entries.Where(x => x.PersonelId == actor.Id);
            if (scope == "delegated") entries = entries.Where(x => DelegatedPersonIds(db, actor).Contains(x.PersonelId));
            if (scope == "team") entries = entries.Where(x => Reviewable(db, actor).Select(e => e.Id).Contains(x.Id));
            var deliveries = db.OvertimeDeliveries.AsNoTracking();
            if (scope != "hr") deliveries = deliveries.Where(x => x.ManagerId == actor.Id);
            var deliveryRows = await (from delivery in deliveries
                join period in db.OvertimePeriods on delivery.PeriodId equals period.Id
                where period.EndDate >= start.Date && period.StartDate <= end.Date
                orderby delivery.UnitName
                select new
                {
                    Delivery = delivery,
                    EntryCount = db.OvertimeEntries.Count(e => e.UnitId == delivery.UnitId && e.WorkDate >= period.StartDate && e.WorkDate <= period.EndDate),
                    ApprovedEntryCount = db.OvertimeEntries.Count(e => e.UnitId == delivery.UnitId && e.WorkDate >= period.StartDate && e.WorkDate <= period.EndDate && e.Status == OvertimeEntryStatus.Approved)
                }).ToListAsync(cancellationToken);
            foreach (var row in deliveryRows)
            {
                row.Delivery.EntryCount = row.EntryCount;
                row.Delivery.ApprovedEntryCount = row.ApprovedEntryCount;
            }
            var result = new OvertimeWorkspace
            {
                PersonelId = actor.Id, FullName = actor.FullName, IsHr = actor.IsHr, IsManager = actor.ManagedUnits.Count > 0 || actor.EscalatedUnits.Count > 0,
                DelegatedPeople = await PersonOptions(db, DelegatedPersonIds(db, actor)).ToListAsync(cancellationToken),
                Entries = await entries.OrderByDescending(x => x.WorkDate).ThenBy(x => x.FullName).ToListAsync(cancellationToken),
                Periods = await PeriodHeaders(db).Where(x => x.EndDate >= start.Date && x.StartDate <= end.Date)
                    .OrderByDescending(x => x.StartDate).ToListAsync(cancellationToken),
                Deliveries = deliveryRows.Select(x => x.Delivery).ToList()
            };
            if (scope == "hr")
                result.ExternalInstitutions = await db.Personel_Table.AsNoTracking()
                    .Where(x => x.Tipi == PersonelTipi.KadroHarici && x.IsBilgileri.CalismaDurumu == CalismaDurumu.Calisiyor)
                    .OrderBy(x => x.Adi).ThenBy(x => x.Soyadi)
                    .Select(x => new OvertimeExternalInstitution(x.Id, x.Adi + " " + x.Soyadi, x.IsBilgileri.KadroHariciKurum)).ToListAsync(cancellationToken);
            if (scope == "hr")
                // Legacy rows can have null names despite Required; filter before materialization.
                result.Titles = await (from title in db.UnvanTanim_Table.AsNoTracking().Where(x => !string.IsNullOrWhiteSpace(x.Adi))
                    join schedule in db.OvertimeSchedules.AsNoTracking() on title.Id equals schedule.TitleId into schedules
                    from schedule in schedules.DefaultIfEmpty()
                    orderby title.Adi
                    select new OvertimeTitleOption(title.Id, title.Adi ?? "", schedule == null ? null : schedule.WeeklyHours))
                    .ToListAsync(cancellationToken);
            return result;
        }, cancellationToken);

    private static async Task EnsureOpen(ApplicationDbContext db, int unitId, DateTime date, CancellationToken ct)
    {
        var period = await db.OvertimePeriods.Where(x => x.StartDate <= date && x.EndDate >= date).SingleOrDefaultAsync(ct);
        if (period is null) return;
        Require(period.Status == OvertimePeriodStatus.Collecting, "Bu tarih imzaya çıkarılmış veya kesinleşmiş bir döneme ait.");
        var delivery = await db.OvertimeDeliveries.SingleOrDefaultAsync(x => x.PeriodId == period.Id && x.UnitId == unitId, ct);
        Require(delivery is not null, "Birim bu dönemin kapsamında değil. İK ile görüşün.");
        Require(delivery!.Status == OvertimeDeliveryStatus.Requested, "Birim çizelgesi imzaya çıkarıldı. Değişiklik için İK'nın birimi yeniden açması gerekir.");
    }

    public Task<Result<int>> SaveEntryAsync(OvertimeEntryInput input, CancellationToken cancellationToken = default)
        => Run(true, async (db, actor) =>
        {
            if (!input.EndWorkDate.HasValue || input.EndWorkDate.Value.Date == input.WorkDate.Date)
                return await SaveDayEntry(db, actor, input, cancellationToken);

            Require(input.Intervals.Count == 1, "Gece yarısını aşan çalışma için tek saat aralığı girin.");
            var interval = input.Intervals[0];
            var start = input.WorkDate.Date.AddMinutes(interval.StartMinute);
            var end = input.EndWorkDate.Value.Date.AddMinutes(interval.End.Hour * 60 + interval.End.Minute);
            Require(end > start && end - start <= TimeSpan.FromHours(24), "Bitiş başlangıçtan sonra olmalı; tek çalışma aralığı 24 saati aşmamalıdır.");
            var firstId = 0;
            // Save all calendar-day parts in the same transaction; a conflict must roll back every part.
            while (start < end)
            {
                var until = end < start.Date.AddDays(1) ? end : start.Date.AddDays(1);
                var day = new OvertimeEntryInput
                {
                    PersonelId = input.PersonelId,
                    Id = firstId == 0 ? input.Id : 0,
                    Version = firstId == 0 ? input.Version : [],
                    WorkDate = start.Date, Reason = input.Reason, Submit = input.Submit,
                    Intervals = [new OvertimeTimeInput
                    {
                        Start = TimeOnly.FromDateTime(start), End = TimeOnly.FromDateTime(until),
                        EndsNextDay = until.Date > start.Date
                    }]
                };
                var id = await SaveDayEntry(db, actor, day, cancellationToken);
                if (firstId == 0) firstId = id;
                start = until;
            }
            return firstId;
        }, cancellationToken);

    private static async Task<int> SaveDayEntry(ApplicationDbContext db, Actor actor, OvertimeEntryInput input, CancellationToken cancellationToken)
    {
        var personelId = input.PersonelId ?? actor.Id;
        await RequireEntryAccess(db, actor, personelId, cancellationToken);
        Require(!string.IsNullOrWhiteSpace(input.Reason) && input.Reason.Trim().Length <= 2000, "Çalışma sebebi zorunludur ve en fazla 2000 karakter olabilir.");
        Require(input.Intervals.Count is > 0 and <= 12, "Bir gün için 1–12 çalışma aralığı girin.");
        var intervals = input.Intervals.OrderBy(x => x.StartMinute).ToList();
        foreach (var interval in intervals)
        {
            Require(interval.EndMinute > interval.StartMinute && interval.EndMinute - interval.StartMinute <= 1440, "Bitiş başlangıçtan sonra olmalı; aralık 24 saati aşmamalıdır.");
            Require(interval.EndMinute <= 1440, "Çalışma aralığının başlangıç ve bitiş tarihlerini kontrol edin.");
        }
        Require(!intervals.Zip(intervals.Skip(1)).Any(x => x.First.EndMinute > x.Second.StartMinute), "Çalışma aralıkları çakışamaz.");
        var entry = input.Id == 0 ? null : await db.OvertimeEntries.Include(x => x.Intervals)
            .SingleOrDefaultAsync(x => x.Id == input.Id && x.PersonelId == personelId, cancellationToken);
        if (input.Id != 0)
        {
            Require(entry is not null, "Kayıt bulunamadı.");
            CheckVersion(entry!.Version, input.Version);
            Require(entry.Status is OvertimeEntryStatus.Draft or OvertimeEntryStatus.Correction, "Yalnızca taslak veya düzeltmeye gönderilen kayıt düzenlenebilir.");
            Require(entry.PeriodId is null, "İmzaya çıkarılan kayıt değiştirilemez.");
            await EnsureOpen(db, entry.UnitId, entry.WorkDate, cancellationToken);
        }
        var info = await db.IsBilgileri_Table.AsNoTracking().Include(x => x.BirimTanim).Include(x => x.UnvanTanim)
            .SingleOrDefaultAsync(x => x.PersonelId == personelId, cancellationToken);
        Require(info?.BirimId is not null && info.UnvanId is not null && info.BirimTanim?.Aktif == true, "Aktif birim ve unvan bilgilerinizin tamamlanması gerekiyor.");
        var hours = await db.OvertimeSchedules.Where(x => x.TitleId == info!.UnvanId).Select(x => (int?)x.WeeklyHours).SingleOrDefaultAsync(cancellationToken);
        Require(hours is 40 or 45, "Unvanınızın 40/45 saat çalışma grubu İK tarafından tanımlanmalıdır.");
        await EnsureOpen(db, info!.BirimId!.Value, input.WorkDate.Date, cancellationToken);
        var existing = await db.OvertimeEntries.Where(x => x.PersonelId == personelId && x.WorkDate == input.WorkDate.Date && x.Id != input.Id)
            .Select(x => new { x.Status, x.PeriodId }).SingleOrDefaultAsync(cancellationToken);
        if (existing is not null)
        {
            var message = existing.PeriodId.HasValue
                ? "Bu tarihte imza çizelgesine alınmış bir mesai kaydınız var. Değişiklik için İK ile görüşün."
                : existing.Status switch
                {
                    OvertimeEntryStatus.Approved => "Bu tarihte onaylı bir mesai kaydınız var. Ek çalışma girmek için amirinizden mevcut kaydı düzeltmeye göndermesini isteyin.",
                    OvertimeEntryStatus.Pending => "Bu tarihte amir onayında bir kaydınız var. Mesailerim listesinden Geri Çek işlemini yapıp Düzenle ile ek saat aralığını girin.",
                    OvertimeEntryStatus.Rejected => "Bu tarihte reddedilmiş bir mesai kaydınız var. Yeniden giriş için amirinizle görüşün.",
                    _ => "Bu tarihte bir mesai kaydınız var. Mesailerim listesindeki Düzenle düğmesiyle aynı güne ek saat aralığı ekleyin."
                };
            Require(false, message);
        }
        var person = await db.Personel_Table.AsNoTracking().SingleAsync(x => x.Id == personelId, cancellationToken);
        var isNew = entry is null;
        entry ??= new OvertimeEntry { PersonelId = personelId };
        var before = isNew ? null : EntrySnapshot(entry);
        entry.WorkDate = input.WorkDate.Date;
        entry.UnitId = info.BirimId.Value;
        entry.UnitName = info.BirimTanim!.Adi ?? "";
        entry.Title = info.UnvanTanim?.Adi ?? "";
        entry.NormalWeeklyMinutes = hours!.Value * 60;
        entry.FullName = $"{person.Adi} {person.Soyadi}";
        entry.SicilNo = person.SicilNo;
        entry.PersonnelType = person.Tipi;
        var institution = PersonnelInstitution.Normalize(info.KadroHariciKurum);
        Require(person.Tipi != PersonelTipi.KadroHarici || institution.Length is > 0 and <= 200,
            "Kadro harici personelin kurum bilgisi İK tarafından tanımlanmalıdır.");
        entry.KadroHariciKurum = person.Tipi == PersonelTipi.KadroHarici ? institution : null;
        entry.Reason = input.Reason.Trim();
        entry.Status = input.Submit ? OvertimeEntryStatus.Pending : OvertimeEntryStatus.Draft;
        entry.ReviewReason = null;
        db.OvertimeIntervals.RemoveRange(entry.Intervals);
        entry.Intervals = intervals.Select(x => new OvertimeInterval { StartMinute = x.StartMinute, EndMinute = x.EndMinute }).ToList();
        if (isNew) db.OvertimeEntries.Add(entry);
        else db.Entry(entry).Property(x => x.Reason).IsModified = true;
        await db.SaveChangesAsync(cancellationToken);
        Audit(db, input.Submit ? "Amire gönderildi" : "Taslak kaydedildi", JsonSerializer.Serialize(new { Before = before, After = EntrySnapshot(entry) }), entry.Id);
        return entry.Id;
    }

    private static object EntrySnapshot(OvertimeEntry entry) => new
    {
        entry.PersonelId, entry.FullName, entry.PersonnelType, entry.KadroHariciKurum, entry.WorkDate, entry.Reason, entry.Status, entry.UnitId, entry.UnitName, entry.Title, entry.NormalWeeklyMinutes,
        Intervals = entry.Intervals.Select(x => new { x.StartMinute, x.EndMinute }).ToArray()
    };

    public Task<Result<bool>> WithdrawAsync(int entryId, byte[] version, CancellationToken cancellationToken = default)
        => Run(true, async (db, actor) =>
        {
            var entry = await db.OvertimeEntries.SingleOrDefaultAsync(x => x.Id == entryId &&
                (x.PersonelId == actor.Id || DelegatedPersonIds(db, actor).Contains(x.PersonelId)), cancellationToken);
            Require(entry is not null, "Kayıt bulunamadı.");
            CheckVersion(entry!.Version, version);
            Require(entry.PeriodId is null && entry.Status is OvertimeEntryStatus.Draft or OvertimeEntryStatus.Correction or OvertimeEntryStatus.Pending,
                "Bu kayıt geri çekilemez.");
            await EnsureOpen(db, entry.UnitId, entry.WorkDate, cancellationToken);
            entry.Status = OvertimeEntryStatus.Draft;
            Audit(db, "Kayıt geri çekildi", "Giriş yetkisi bulunan kullanıcı düzenlemek için kaydı taslağa aldı.", entryId);
            return true;
        }, cancellationToken);

    public Task<Result<bool>> ReviewAsync(int entryId, byte[] version, OvertimeEntryStatus decision, string reason, CancellationToken cancellationToken = default)
        => Run(true, async (db, actor) =>
        {
            Require(decision is OvertimeEntryStatus.Approved or OvertimeEntryStatus.Rejected or OvertimeEntryStatus.Correction, "Geçersiz karar.");
            Require(reason.Length <= 2000 && (decision == OvertimeEntryStatus.Approved || !string.IsNullOrWhiteSpace(reason)), "Ret veya düzeltme gerekçesi zorunludur (en fazla 2000 karakter).");
            var entry = await Reviewable(db, actor).SingleOrDefaultAsync(x => x.Id == entryId, cancellationToken);
            Require(entry is not null && entry.PersonelId != actor.Id, "Bu kayda amir olarak işlem yapamazsınız; kendi kaydınızı onaylayamazsınız.");
            CheckVersion(entry!.Version, version);
            Require(entry.PeriodId is null && (entry.Status == OvertimeEntryStatus.Pending ||
                (entry.Status == OvertimeEntryStatus.Approved && decision == OvertimeEntryStatus.Correction)),
                "Onay bekleyen kayıt incelenebilir; onaylı kayıt yalnızca çizelgeye alınmadan önce düzeltmeye gönderilebilir.");
            await EnsureOpen(db, entry.UnitId, entry.WorkDate, cancellationToken);
            entry.Status = decision;
            entry.ReviewReason = reason.Trim();
            Audit(db, decision.Label(), reason.Trim(), entryId);
            return true;
        }, cancellationToken);

    public Task<Result<List<OvertimeEvent>>> GetEntryHistoryAsync(int entryId, CancellationToken cancellationToken = default)
        => Run(false, async (db, actor) =>
        {
            Require(await db.OvertimeEntries.AnyAsync(x => x.Id == entryId &&
                (actor.IsHr || x.PersonelId == actor.Id || DelegatedPersonIds(db, actor).Contains(x.PersonelId) || Reviewable(db, actor).Select(e => e.Id).Contains(x.Id)), cancellationToken), "Kayıt bulunamadı veya erişim yetkiniz yok.");
            return await db.OvertimeEvents.AsNoTracking().Where(x => x.EntryId == entryId).OrderBy(x => x.Id).ToListAsync(cancellationToken);
        }, cancellationToken);

    public Task<Result<int>> CreatePeriodAsync(string name, DateTime start, DateTime end, DateTime dueDate, CancellationToken cancellationToken = default)
        => Run(true, async (db, actor) =>
        {
            Hr(actor);
            Require(!string.IsNullOrWhiteSpace(name) && name.Trim().Length <= 200, "Dönem adı zorunludur (en fazla 200 karakter).");
            Require(start.DayOfWeek == DayOfWeek.Monday && end.DayOfWeek == DayOfWeek.Sunday && end.Date >= start.Date && (end - start).TotalDays <= 370,
                "Dönem Pazartesi başlayıp Pazar bitmeli, en az bir tam hafta ve en fazla bir yıl olmalıdır.");
            Require(dueDate.Date >= end.Date, "Son teslim tarihi dönem bitişinden önce olamaz.");
            Require(!await db.OvertimePeriods.AnyAsync(x => x.StartDate <= end.Date && x.EndDate >= start.Date, cancellationToken), "Bu tarihlerle çakışan bir dönem var.");
            var units = await db.BirimTanim_Table.AsNoTracking().Where(x => x.Aktif == true &&
                (db.IsBilgileri_Table.Any(i => i.BirimId == x.Id) || db.OvertimeEntries.Any(e => e.UnitId == x.Id && e.WorkDate >= start.Date && e.WorkDate <= end.Date)))
                .ToListAsync(cancellationToken);
            Require(units.Count > 0, "Döneme alınabilecek aktif personel birimi bulunamadı.");
            var period = new OvertimePeriod { Name = name.Trim(), StartDate = start.Date, EndDate = end.Date, DueDate = dueDate.Date };
            db.OvertimePeriods.Add(period);
            await db.SaveChangesAsync(cancellationToken);
            db.OvertimeDeliveries.AddRange(units.Select(x => new OvertimeDelivery { PeriodId = period.Id, UnitId = x.Id, UnitName = x.Adi ?? "", ManagerId = x.AmirId }));
            Audit(db, "Toplama çağrısı açıldı", $"{period.Name}: {start:dd.MM.yyyy}–{end:dd.MM.yyyy}; son teslim {dueDate:dd.MM.yyyy}", periodId: period.Id);
            return period.Id;
        }, cancellationToken);

    private static async Task<OvertimePeriod> CollectingPeriod(ApplicationDbContext db, int id, CancellationToken ct)
    {
        var period = await db.OvertimePeriods.SingleOrDefaultAsync(x => x.Id == id, ct);
        Require(period?.Status == OvertimePeriodStatus.Collecting, "Dönem toplama aşamasında değil.");
        return period!;
    }

    public Task<Result<bool>> IssueUnitAsync(int deliveryId, byte[] version, CancellationToken cancellationToken = default)
        => Run(true, async (db, actor) =>
        {
            var delivery = await db.OvertimeDeliveries.SingleOrDefaultAsync(x => x.Id == deliveryId, cancellationToken);
            Require(delivery is not null && (delivery.ManagerId == actor.Id || actor.IsHr), "Birim çizelgesine erişim yetkiniz yok.");
            CheckVersion(delivery!.Version, version);
            var period = await CollectingPeriod(db, delivery.PeriodId, cancellationToken);
            Require(delivery.Status == OvertimeDeliveryStatus.Requested, "Birim çizelgesi zaten oluşturulmuş.");
            var entries = await db.OvertimeEntries.Where(x => x.UnitId == delivery.UnitId && x.WorkDate >= period.StartDate && x.WorkDate <= period.EndDate).ToListAsync(cancellationToken);
            Require(entries.Any(x => x.Status == OvertimeEntryStatus.Approved), "Bu birimde dönem içinde onaylı fazla mesai kaydı bulunmuyor.");
            Require(entries.All(x => x.Status is OvertimeEntryStatus.Approved or OvertimeEntryStatus.Rejected), "Birimde taslak, onay veya düzeltme bekleyen kayıtlar var. Önce bu kayıtları sonuçlandırın.");
            Require(entries.All(x => x.PeriodId == null), "Başka bir çizelgeye alınmış kayıt var.");
            foreach (var entry in entries.Where(x => x.Status == OvertimeEntryStatus.Approved)) entry.PeriodId = period.Id;
            delivery.Status = OvertimeDeliveryStatus.Issued;
            delivery.Revision++;
            delivery.IssuedAt = DateTime.Now;
            Audit(db, "Birim çizelgesi sabitlendi", $"{delivery.UnitName}; {entries.Count(x => x.Status == OvertimeEntryStatus.Approved)} onaylı kayıt", periodId: period.Id);
            return true;
        }, cancellationToken);

    public Task<Result<bool>> ReceiveUnitAsync(int deliveryId, byte[] version, CancellationToken cancellationToken = default)
        => Run(true, async (db, actor) =>
        {
            Hr(actor);
            var delivery = await db.OvertimeDeliveries.SingleOrDefaultAsync(x => x.Id == deliveryId, cancellationToken);
            Require(delivery is not null, "Birim teslim kaydı bulunamadı.");
            CheckVersion(delivery!.Version, version);
            await CollectingPeriod(db, delivery.PeriodId, cancellationToken);
            Require(delivery.Status == OvertimeDeliveryStatus.Issued, "Önce birim çizelgesi oluşturulmalıdır.");
            delivery.Status = OvertimeDeliveryStatus.Received;
            delivery.ReceivedAt = DateTime.Now;
            Audit(db, "İmzalı birim çizelgesi teslim alındı", delivery.UnitName, periodId: delivery.PeriodId);
            return true;
        }, cancellationToken);

    public Task<Result<bool>> ReopenUnitAsync(int deliveryId, byte[] version, string reason, CancellationToken cancellationToken = default)
        => Run(true, async (db, actor) =>
        {
            Hr(actor);
            Require(!string.IsNullOrWhiteSpace(reason) && reason.Length <= 2000, "Yeniden açma gerekçesi zorunludur (en fazla 2000 karakter).");
            var delivery = await db.OvertimeDeliveries.SingleOrDefaultAsync(x => x.Id == deliveryId, cancellationToken);
            Require(delivery is not null, "Birim teslim kaydı bulunamadı.");
            CheckVersion(delivery!.Version, version);
            await CollectingPeriod(db, delivery.PeriodId, cancellationToken);
            Require(delivery.Status != OvertimeDeliveryStatus.Requested, "Birim zaten açık.");
            var entries = await db.OvertimeEntries.Where(x => x.PeriodId == delivery.PeriodId && x.UnitId == delivery.UnitId).ToListAsync(cancellationToken);
            foreach (var entry in entries)
            {
                entry.PeriodId = null;
                entry.Status = OvertimeEntryStatus.Correction;
                entry.ReviewReason = reason.Trim();
                Audit(db, "İK düzeltmeye açtı", reason.Trim(), entry.Id, delivery.PeriodId);
            }
            delivery.Status = OvertimeDeliveryStatus.Requested;
            delivery.IssuedAt = delivery.ReceivedAt = null;
            Audit(db, "Birim yeniden açıldı", $"{delivery.UnitName}: {reason.Trim()}. Önceki çıktı geçersizdir.", periodId: delivery.PeriodId);
            return true;
        }, cancellationToken);

    private static List<OvertimePersonResult> CalculateResults(int periodId, List<OvertimeEntry> entries, DateTime start, DateTime end)
        => entries.GroupBy(x => x.PersonelId).Select(person =>
        {
            Require(person.Select(x => x.PersonnelType).Distinct().Count() == 1,
                $"{person.First().FullName}: Dönem içinde farklı personel tipleriyle kayıt var. Kayıtları İK ile kontrol edin.");
            Require(person.Where(x => x.PersonnelType == PersonelTipi.KadroHarici).Select(x => PersonnelInstitution.Key(x.KadroHariciKurum)).Distinct().Count() <= 1,
                $"{person.First().FullName}: Dönem içinde farklı kurumlarla kayıt var. Kayıtları İK ile kontrol edin.");
            List<OvertimeWeek> weeks;
            try { weeks = OvertimeCalculator.Calculate(person, start, end); }
            catch (ArgumentException ex) { throw new RuleException(Error.Validation($"{person.First().FullName}: {ex.Message}")); }
            var raw = weeks.Sum(x => x.OvertimeMinutes);
            return new OvertimePersonResult
            {
                PeriodId = periodId, PersonelId = person.Key, FullName = person.First().FullName, SicilNo = person.First().SicilNo, PersonnelType = person.First().PersonnelType, KadroHariciKurum = person.First().KadroHariciKurum,
                RawMinutes = raw, RoundedMinutes = OvertimeCalculator.RoundTotal(raw), WeeksJson = JsonSerializer.Serialize(weeks)
            };
        }).OrderBy(x => x.FullName).ToList();

    public Task<Result<bool>> PrepareSignatureAsync(int periodId, byte[] version, CancellationToken cancellationToken = default)
        => Run(true, async (db, actor) =>
        {
            Hr(actor);
            var period = await CollectingPeriod(db, periodId, cancellationToken);
            CheckVersion(period.Version, version);
            Require(await db.OvertimeDeliveries.AnyAsync(x => x.PeriodId == periodId, cancellationToken) &&
                !await db.OvertimeDeliveries.AnyAsync(x => x.PeriodId == periodId && x.Status != OvertimeDeliveryStatus.Received &&
                    db.OvertimeEntries.Any(e => e.UnitId == x.UnitId && e.WorkDate >= period.StartDate && e.WorkDate <= period.EndDate && e.Status == OvertimeEntryStatus.Approved), cancellationToken), "Onaylı mesaisi bulunan bütün birimlerin imzalı çizelgeleri teslim alınmalıdır.");
            Require(!await db.OvertimeEntries.AnyAsync(x => x.WorkDate >= period.StartDate && x.WorkDate <= period.EndDate &&
                (x.Status != OvertimeEntryStatus.Rejected && (x.Status != OvertimeEntryStatus.Approved || x.PeriodId != periodId)), cancellationToken), "Dönem içinde çizelge dışında kalan veya sonuçlandırılmamış kayıtlar var.");
            var entries = await db.OvertimeEntries.Include(x => x.Intervals).Where(x => x.PeriodId == periodId).ToListAsync(cancellationToken);
            await ResolvePersonnelTypesAsync(db, entries, cancellationToken);
            Require(entries.All(x => x.PersonnelType is PersonelTipi.Kadrolu or PersonelTipi.KadroHarici),
                "Personel tipi eksik veya geçersiz kayıtlar var. İmzaya hazırlamadan önce İK personel tiplerini tanımlamalıdır.");
            Require(entries.All(x => x.PersonnelType != PersonelTipi.KadroHarici || PersonnelInstitution.Normalize(x.KadroHariciKurum).Length is > 0 and <= 200),
                "Kadro harici personelin kurum bilgileri eksik. İmzaya hazırlamadan önce kurum bilgilerini tamamlayın.");
            var results = CalculateResults(periodId, entries, period.StartDate, period.EndDate);
            db.OvertimeResults.AddRange(results);
            period.Status = OvertimePeriodStatus.AwaitingSignature;
            period.SignatureRevision++;
            Audit(db, "GMY ıslak imzasına çıkarıldı", $"{results.Count} personel; hesaplar sabitlendi; kural 2026-11-v1.", periodId: periodId);
            return true;
        }, cancellationToken);

    public Task<Result<bool>> ReopenPeriodAsync(int periodId, byte[] version, string reason, CancellationToken cancellationToken = default)
        => Run(true, async (db, actor) =>
        {
            Hr(actor);
            Require(!string.IsNullOrWhiteSpace(reason) && reason.Length <= 2000, "Geri alma gerekçesi zorunludur (en fazla 2000 karakter).");
            var period = await db.OvertimePeriods.SingleOrDefaultAsync(x => x.Id == periodId, cancellationToken);
            Require(period?.Status == OvertimePeriodStatus.AwaitingSignature, "Yalnızca imzada bekleyen dönem geri alınabilir. Kesinleşmiş dönem değiştirilemez.");
            CheckVersion(period!.Version, version);
            var results = await db.OvertimeResults.Where(x => x.PeriodId == periodId).ToListAsync(cancellationToken);
            Audit(db, "İmza listesi geri alındı", JsonSerializer.Serialize(new { Reason = reason.Trim(), InvalidatedResults = results }), periodId: periodId);
            db.OvertimeResults.RemoveRange(results);
            period.Status = OvertimePeriodStatus.Collecting;
            return true;
        }, cancellationToken);

    public Task<Result<bool>> FinalizeAsync(int periodId, byte[] version, string fileName, byte[] document, CancellationToken cancellationToken = default)
        => Run(true, async (db, actor) =>
        {
            Hr(actor);
            var period = await db.OvertimePeriods.SingleOrDefaultAsync(x => x.Id == periodId, cancellationToken);
            Require(period?.Status == OvertimePeriodStatus.AwaitingSignature, "Yalnızca ıslak imza bekleyen dönem kesinleştirilebilir.");
            CheckVersion(period!.Version, version);
            Require(document.Length is > 0 and <= 10 * 1024 * 1024, "İmzalı belge en fazla 10 MB olabilir.");
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            var isPdf = extension == ".pdf" && document.AsSpan().StartsWith("%PDF-"u8);
            var isPng = extension == ".png" && document.AsSpan().StartsWith(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
            var isJpeg = (extension is ".jpg" or ".jpeg") && document.AsSpan().StartsWith(new byte[] { 255, 216, 255 });
            Require(isPdf || isPng || isJpeg, "İmzalı nüsha PDF, JPEG veya PNG olmalı; dosya içeriği uzantısıyla eşleşmelidir.");
            period.SignedFileName = Path.GetFileName(fileName);
            Require(period.SignedFileName.Length <= 200, "Belge adı en fazla 200 karakter olabilir.");
            period.SignedContentType = isPdf ? "application/pdf" : isPng ? "image/png" : "image/jpeg";
            period.SignedDocument = document;
            period.FinalizedAt = DateTime.Now;
            period.Status = OvertimePeriodStatus.Finalized;
            Audit(db, "İmzalı belge alındı ve dönem kesinleştirildi", period.SignedFileName, periodId: periodId);
            return true;
        }, cancellationToken);

    public Task<Result<bool>> SetScheduleAsync(int titleId, int weeklyHours, CancellationToken cancellationToken = default)
        => Run(true, async (db, actor) =>
        {
            Hr(actor);
            Require(weeklyHours is 40 or 45, "Çalışma grubu 40 veya 45 saat olmalıdır.");
            Require(await db.UnvanTanim_Table.AnyAsync(x => x.Id == titleId, cancellationToken), "Unvan bulunamadı.");
            var schedule = await db.OvertimeSchedules.SingleOrDefaultAsync(x => x.TitleId == titleId, cancellationToken);
            if (schedule is null) db.OvertimeSchedules.Add(new() { TitleId = titleId, WeeklyHours = weeklyHours });
            else schedule.WeeklyHours = weeklyHours;
            Audit(db, "Unvan çalışma grubu tanımlandı", $"Unvan {titleId}: {weeklyHours} saat. Önceden girilmiş kayıtların grubu korunur.");
            return true;
        }, cancellationToken);

    public Task<Result<OvertimeReport>> GetReportAsync(int periodId, int? unitId, int? personelId, CancellationToken cancellationToken = default)
        => Run(false, async (db, actor) =>
        {
            var period = await PeriodHeaders(db).SingleOrDefaultAsync(x => x.Id == periodId, cancellationToken);
            Require(period is not null, "Dönem bulunamadı.");
            Require(!unitId.HasValue || !personelId.HasValue, "Birim ve personel filtresi birlikte kullanılamaz. Birim çizelgesi bütün onaylı kayıtları içermelidir.");
            if (unitId.HasValue)
                Require(actor.IsHr || await db.OvertimeDeliveries.AnyAsync(x => x.PeriodId == periodId && x.UnitId == unitId && x.ManagerId == actor.Id, cancellationToken), "Birim çizelgesini görme yetkiniz yok.");
            else if (!actor.IsHr)
            {
                personelId ??= actor.Id;
                if (personelId != actor.Id && !await DelegatedPersonIds(db, actor).ContainsAsync(personelId.Value, cancellationToken))
                    throw new RuleException(Error.Forbidden("Bu personelin çizelgesini görme yetkiniz bulunmuyor."));
            }
            var entries = db.OvertimeEntries.AsNoTracking().Include(x => x.Intervals)
                .Where(x => x.WorkDate >= period!.StartDate && x.WorkDate <= period.EndDate && x.Status == OvertimeEntryStatus.Approved);
            if (unitId.HasValue) entries = entries.Where(x => x.UnitId == unitId);
            if (personelId.HasValue) entries = entries.Where(x => x.PersonelId == personelId);
            var list = await entries.OrderBy(x => x.FullName).ThenBy(x => x.WorkDate).ToListAsync(cancellationToken);
            await ResolvePersonnelTypesAsync(db, list, cancellationToken);
            var summary = actor.IsHr && !unitId.HasValue && !personelId.HasValue;
            var delivery = unitId.HasValue ? await db.OvertimeDeliveries.AsNoTracking().SingleOrDefaultAsync(x => x.PeriodId == periodId && x.UnitId == unitId, cancellationToken) : null;
            var frozenPersonal = personelId.HasValue && list.Count > 0 && list.All(x => x.PeriodId == periodId);
            var result = new OvertimeReport
            {
                Period = period!, Entries = list, CanSeeSummary = summary,
                IsPreview = summary ? period!.Status == OvertimePeriodStatus.Collecting : unitId.HasValue ? delivery?.Status is null or OvertimeDeliveryStatus.Requested : !frozenPersonal,
                Reference = unitId.HasValue ? $"FM-{periodId}/B-{unitId}/R-{delivery?.Revision ?? 0}" : summary ? $"FM-{periodId}/R-{period!.SignatureRevision}" : $"FM-{periodId}/P-{personelId}"
            };
            if (!summary)
                result.UnitSignatures = await ReportSignaturesAsync(db, list.Select(x => x.UnitId).Distinct().ToList(), cancellationToken);
            else
            {
                var directorUnits = await db.BirimTanim_Table.AsNoTracking().Where(x => x.Aktif == true && x.AmirId.HasValue)
                    .Select(x => new { x.Id, x.Adi, x.AmirId }).ToListAsync(cancellationToken);
                var matchingDirectorUnits = directorUnits.Where(x =>
                {
                    var name = NormalizeOrganizationName(x.Adi);
                    return name.Contains("PERSONELVEIDARIISLER") || name.Contains("PERSONELIDARIISLER") || name.Contains("PERVEIDISL");
                }).ToList();
                var directorIds = matchingDirectorUnits.Select(x => x.AmirId!.Value).Distinct().ToList();
                if (directorIds.Count == 1)
                {
                    var name = await db.Personel_Table.AsNoTracking().Where(x => x.Id == directorIds[0])
                        .Select(x => (x.Adi ?? "") + " " + (x.Soyadi ?? "")).SingleOrDefaultAsync(cancellationToken);
                    if (!string.IsNullOrWhiteSpace(name)) result.PersonnelDirectorName = name.Trim();
                    var signatures = await ReportSignaturesAsync(db, matchingDirectorUnits.Select(x => x.Id).ToList(), cancellationToken);
                    var deputyNames = signatures.Values.Select(x => x.DeputyGeneralManagerName).Distinct().ToList();
                    if (deputyNames.Count == 1) result.SummaryDeputyGeneralManagerName = deputyNames[0];
                }
            }
            if (personelId.HasValue && personelId != actor.Id &&
                await DelegatedPersonIds(db, actor).ContainsAsync(personelId.Value, cancellationToken))
            {
                result.DelegateName = actor.FullName;
                var info = await db.IsBilgileri_Table.AsNoTracking().Where(x => x.PersonelId == actor.Id)
                    .Select(x => new
                    {
                        Duty = x.GorevTanim != null ? x.GorevTanim.Adi : null,
                        Title = x.UnvanTanim != null ? x.UnvanTanim.Adi : null
                    }).SingleOrDefaultAsync(cancellationToken);
                result.DelegateDuty = !string.IsNullOrWhiteSpace(info?.Duty) ? info.Duty
                    : !string.IsNullOrWhiteSpace(info?.Title) ? info.Title : "Görev tanımlanmamış";
            }
            if (summary || personelId == actor.Id)
            {
                if (period!.Status == OvertimePeriodStatus.Collecting)
                    result.Results = CalculateResults(periodId, list, period.StartDate, period.EndDate);
                else
                    result.Results = await db.OvertimeResults.AsNoTracking().Where(x => x.PeriodId == periodId && (summary || x.PersonelId == actor.Id)).OrderBy(x => x.FullName).ToListAsync(cancellationToken);
                foreach (var item in result.Results.Where(x => x.PersonnelType is null))
                    item.PersonnelType = list.FirstOrDefault(x => x.PersonelId == item.PersonelId)?.PersonnelType;
                foreach (var item in result.Results.Where(x => x.PersonnelType == PersonelTipi.KadroHarici && string.IsNullOrWhiteSpace(x.KadroHariciKurum)))
                    item.KadroHariciKurum = list.FirstOrDefault(x => x.PersonelId == item.PersonelId)?.KadroHariciKurum;
            }
            if (actor.IsHr)
                result.Events = await db.OvertimeEvents.AsNoTracking().Where(x => x.PeriodId == periodId).OrderBy(x => x.Id).ToListAsync(cancellationToken);
            return result;
        }, cancellationToken);

    private static async Task ResolvePersonnelTypesAsync(ApplicationDbContext db, List<OvertimeEntry> entries, CancellationToken ct)
    {
        var ids = entries.Where(x => x.PersonnelType is null || (x.PersonnelType == PersonelTipi.KadroHarici && string.IsNullOrWhiteSpace(x.KadroHariciKurum)))
            .Select(x => x.PersonelId).Distinct().ToList();
        if (ids.Count == 0) return;
        var types = await db.Personel_Table.AsNoTracking().Where(x => ids.Contains(x.Id))
            .Select(x => new { x.Id, x.Tipi, x.IsBilgileri.KadroHariciKurum }).ToDictionaryAsync(x => x.Id, ct);
        foreach (var entry in entries)
        {
            if (!types.TryGetValue(entry.PersonelId, out var info)) continue;
            entry.PersonnelType ??= info.Tipi;
            if (entry.PersonnelType == PersonelTipi.KadroHarici && string.IsNullOrWhiteSpace(entry.KadroHariciKurum))
                entry.KadroHariciKurum = PersonnelInstitution.Normalize(info.KadroHariciKurum);
        }
    }

    private static async Task<Dictionary<int, OvertimeUnitSignatures>> ReportSignaturesAsync(
        ApplicationDbContext db, List<int> unitIds, CancellationToken ct)
    {
        var result = new Dictionary<int, OvertimeUnitSignatures>();
        if (unitIds.Count == 0) return result;
        var units = await db.BirimTanim_Table.AsNoTracking().Where(x => x.Aktif == true).ToDictionaryAsync(x => x.Id, ct);
        var managerIds = new HashSet<int>();
        foreach (var unitId in unitIds)
        {
            var visited = new HashSet<int>();
            var currentId = unitId;
            while (visited.Add(currentId) && units.TryGetValue(currentId, out var unit))
            {
                if (unit.AmirId.HasValue) managerIds.Add(unit.AmirId.Value);
                if (!unit.ParentId.HasValue) break;
                currentId = unit.ParentId.Value;
            }
        }
        var managers = await db.Personel_Table.AsNoTracking().Where(x => managerIds.Contains(x.Id))
            .Select(x => new
            {
                x.Id,
                Name = (x.Adi ?? "") + " " + (x.Soyadi ?? ""),
                Duty = x.IsBilgileri.GorevTanim != null ? x.IsBilgileri.GorevTanim.Adi : null,
                Title = x.IsBilgileri.UnvanTanim != null ? x.IsBilgileri.UnvanTanim.Adi : null
            }).ToDictionaryAsync(x => x.Id, ct);
        foreach (var unitId in unitIds)
        {
            var signatures = new OvertimeUnitSignatures();
            result[unitId] = signatures;
            if (!units.TryGetValue(unitId, out var unit)) continue;
            if (unit.AmirId.HasValue && managers.TryGetValue(unit.AmirId.Value, out var manager))
            {
                signatures.ManagerName = manager.Name.Trim();
                signatures.ManagerDuty = !string.IsNullOrWhiteSpace(manager.Duty) ? manager.Duty
                    : !string.IsNullOrWhiteSpace(manager.Title) ? manager.Title : "Birim Amiri";
            }
            var visited = new HashSet<int> { unitId };
            var parentId = unit.ParentId;
            while (parentId.HasValue && visited.Add(parentId.Value) && units.TryGetValue(parentId.Value, out var parent))
            {
                if (parent.AmirId.HasValue && managers.TryGetValue(parent.AmirId.Value, out var superior) &&
                    (IsDeputyGeneralManager(parent.Adi) || IsDeputyGeneralManager(superior.Duty) || IsDeputyGeneralManager(superior.Title)))
                {
                    signatures.DeputyGeneralManagerName = superior.Name.Trim();
                    break;
                }
                parentId = parent.ParentId;
            }
        }
        return result;
    }

    private static bool IsDeputyGeneralManager(string? name)
    {
        var normalized = NormalizeOrganizationName(name);
        return normalized.Contains("GENELMUDURYARD") || normalized.Contains("GENELMUDURYRD") || normalized == "GMY";
    }

    private static string NormalizeOrganizationName(string? name)
        => new((name ?? "").ToUpperInvariant().Replace('İ', 'I').Replace('Ü', 'U').Replace('Ö', 'O')
            .Replace('Ş', 'S').Replace('Ğ', 'G').Replace('Ç', 'C')
            .Where(char.IsLetterOrDigit).ToArray());

    public Task<Result<(string Name, string ContentType, byte[] Data)>> GetSignedDocumentAsync(int periodId, CancellationToken cancellationToken = default)
        => Run(false, async (db, actor) =>
        {
            Hr(actor);
            var file = await db.OvertimePeriods.AsNoTracking().Where(x => x.Id == periodId)
                .Select(x => new { x.SignedFileName, x.SignedContentType, x.SignedDocument }).SingleOrDefaultAsync(cancellationToken);
            Require(file?.SignedDocument is not null, "İmzalı belge bulunamadı.");
            return (file!.SignedFileName!, file.SignedContentType!, file.SignedDocument!);
        }, cancellationToken);
}
