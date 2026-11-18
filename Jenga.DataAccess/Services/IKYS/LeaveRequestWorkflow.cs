using System.Data;
using Jenga.DataAccess.Data;
using Jenga.DataAccess.Services.Common;
using Jenga.Models.Enums;
using Jenga.Models.IKYS;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Status = Jenga.Models.IKYS.IzinTalep.OnayDurumuTipi;

namespace Jenga.DataAccess.Services.IKYS;

public sealed partial class LeaveRequestWorkflow(
    IDbContextFactory<ApplicationDbContext> dbFactory,
    IServiceAuthorizationContext authorization,
    ILogService log,
    IEmailService email,
    IConfiguration configuration) : ILeaveRequestWorkflow
{
    private static readonly Error AccessDenied = Error.Forbidden("Bu izin talebi işlemi için yetkiniz bulunmuyor.");
    private static readonly Error MissingRequest = Error.NotFound("İzin talebi bulunamadı.");
    private static readonly Error ChangedRequest = Error.Conflict("Talebin durumu değişmiş veya talep kayıtlara işlenmiş. Listeyi yenileyiniz.");

    public Task<Result<LeaveRequestList>> ListAsync(bool personnelOffice, LeaveListFilter filter, Status? status, int page,
        CancellationToken cancellationToken = default) => RunAsync<LeaveRequestList>("List", async db =>
    {
        var ct = cancellationToken;
        var actor = await FindActorAsync(db, personnelOffice, Operation.View, ct);
        if (actor.IsFailure) return actor.Error;

        var requests = db.IzinTalep_Table.AsNoTracking().Where(x => x.Aktif == true);
        if (!personnelOffice) requests = requests.Where(x => x.PersonelId == actor.Value.Id);
        if (filter == LeaveListFilter.General) requests = requests.Where(x => x.IzinTipi != (int)LeaveType.Personal);
        if (filter == LeaveListFilter.Personal) requests = requests.Where(x => x.IzinTipi == (int)LeaveType.Personal);
        if (status.HasValue) requests = requests.Where(x => x.OnayDurumu == status);

        var total = await requests.CountAsync(ct);
        var offset = Math.Clamp(page, 0, Math.Max(0, (total - 1) / ILeaveRequestWorkflow.PageSize));
        var ordered = requests
            .OrderBy(x => x.OnayDurumu == Status.IslemBekliyor || x.OnayDurumu == Status.DilekceBekleniyor ? 0 : x.OnayDurumu == Status.KontrolEdildi ? 1 : 2)
            .ThenByDescending(x => x.BaslangicTarihi).ThenByDescending(x => x.Id);
        var items = await Rows(db, ordered.Skip(offset * ILeaveRequestWorkflow.PageSize).Take(ILeaveRequestWorkflow.PageSize))
            .ToListAsync(ct);
        return new LeaveRequestList(items, total);
    }, cancellationToken);

    public Task<Result<LeaveRequestDetail>> DetailAsync(int id, bool personnelOffice, CancellationToken cancellationToken = default) =>
        RunAsync<LeaveRequestDetail>("Detail", async db =>
        {
            var ct = cancellationToken;
            var actor = await FindActorAsync(db, personnelOffice, Operation.View, ct);
            if (actor.IsFailure) return actor.Error;
            var request = await FindRequestAsync(db, id, actor.Value, personnelOffice, ct);
            if (request is null) return MissingRequest;

            var row = await Rows(db, db.IzinTalep_Table.AsNoTracking().Where(x => x.Id == id)).SingleAsync(ct);
            var signers = await db.Personel_Table.Where(x => x.Id == request.VekilImza || x.Id == request.AmirImza)
                .Select(x => new LeaveOption(x.Id, x.Adi + " " + x.Soyadi)).ToListAsync(ct);
            var periods = await db.IzinDonem_Table.AsNoTracking().Where(x => x.PersonelId == request.PersonelId &&
                (x.IzinTipi == (int)LeaveType.Paid || x.IzinTipi == (int)LeaveType.Personal))
                .OrderByDescending(x => x.BaslangicTarihi).ThenByDescending(x => x.Id).ToListAsync(ct);
            var movements = await (from movement in db.IzinHareket_Table.AsNoTracking()
                                   join type in db.IzinTanim_Table on movement.IzinTipi equals (int?)type.Id into types
                                   from type in types.DefaultIfEmpty()
                                   where movement.PersonelId == request.PersonelId
                                   orderby movement.BaslangicTarihi descending, movement.Id descending
                                   select new LeaveMovementRow(type.Adi ?? "İzin", movement.IzinTipi, movement.BaslangicTarihi,
                                       movement.BitisTarihi, movement.Sure, movement.Birim)).Take(10).ToListAsync(ct);
            var period = periods.Find(x => x.Id == request.IzinDonemId);
            return new LeaveRequestDetail(row, request.Adres, request.Aciklama,
                signers.Find(x => x.Id == request.VekilImza)?.Name, signers.Find(x => x.Id == request.AmirImza)?.Name,
                periods, movements, DocumentUrl(request, period));
        }, cancellationToken);

    public Task<Result<LeaveRequestForm>> LoadFormAsync(int? id, bool personnelOffice, CancellationToken cancellationToken = default) =>
        RunAsync<LeaveRequestForm>("LoadForm", async db =>
        {
            var ct = cancellationToken;
            var actor = await FindActorAsync(db, personnelOffice, id.HasValue ? Operation.Edit : Operation.Create, ct);
            if (actor.IsFailure) return actor.Error;

            var request = id is { } requestId ? await FindRequestAsync(db, requestId, actor.Value, personnelOffice, ct) : null;
            if (id.HasValue && request is null) return MissingRequest;
            if (request is not null && (!CanEdit(request) || await HasMovementAsync(db, request.Id, ct))) return ChangedRequest;

            var staff = await ActiveStaff(db).OrderBy(x => x.Adi).ThenBy(x => x.Soyadi)
                .Select(x => new LeaveOption(x.Id, x.Adi + " " + x.Soyadi)).ToListAsync(ct);
            var knownTypes = Enum.GetValues<LeaveType>().Select(x => (int)x).ToArray();
            var types = await db.IzinTanim_Table.Where(x => knownTypes.Contains(x.Id))
                .OrderBy(x => x.Id).Select(x => new LeaveOption(x.Id, x.Adi ?? "")).ToListAsync(ct);
            var substitutes = staff.Where(x => x.Id != GeneralManagerId).ToList();
            List<LeaveOption> people = personnelOffice ? [.. staff] : [new(actor.Value.Id, $"{actor.Value.Adi} {actor.Value.Soyadi}")];
            var input = new LeaveRequestInput { PersonelId = personnelOffice ? null : actor.Value.Id, StartDate = DateTime.Today, EndDate = DateTime.Today };
            if (request is { } row)
            {
                if (people.All(x => x.Id != row.PersonelId) && row.Personel is { } owner)
                    people.Add(new(owner.Id, $"{owner.Adi} {owner.Soyadi}"));
                input = new LeaveRequestInput
                {
                    Id = row.Id, PersonelId = row.PersonelId, Type = (LeaveType?)row.IzinTipi,
                    StartDate = row.BaslangicTarihi?.Date, EndDate = row.BitisTarihi?.Date,
                    StartTime = row.BaslangicTarihi is { } start ? TimeOnly.FromDateTime(start) : null,
                    EndTime = row.BitisTarihi is { } end ? TimeOnly.FromDateTime(end) : null,
                    SubstituteId = row.VekilImza is > 0 ? row.VekilImza : null,
                    ManagerId = row.AmirImza is > 0 ? row.AmirImza : null,
                    Address = row.Adres, Description = row.Aciklama, Notify = row.EPostaGonder == true
                };
                var legacySigners = await db.Personel_Table.Where(x => x.Id == input.ManagerId || x.Id == input.SubstituteId)
                    .Select(x => new LeaveOption(x.Id, x.Adi + " " + x.Soyadi)).ToListAsync(ct);
                if (legacySigners.Find(x => x.Id == input.ManagerId) is { } manager && staff.All(x => x.Id != manager.Id))
                    staff.Add(manager);
                if (legacySigners.Find(x => x.Id == input.SubstituteId && x.Id != GeneralManagerId) is { } substitute && substitutes.All(x => x.Id != substitute.Id))
                    substitutes.Add(substitute);
            }
            return new LeaveRequestForm(input, people, types, staff, substitutes);
        }, cancellationToken);

    public Task<Result<LeaveCalculation>> CalculateAsync(LeaveRequestInput input, bool personnelOffice,
        CancellationToken cancellationToken = default) => RunAsync<LeaveCalculation>("Calculate", async db =>
    {
        var ct = cancellationToken;
        var target = await ResolveTargetAsync(db, input, personnelOffice, ct);
        if (target.IsFailure) return target.Error;
        var prepared = await PrepareAsync(db, target.Value.Person, input, target.Value.Request?.OnayDurumu == Status.DilekceBekleniyor, ct);
        return prepared.IsSuccess ? Result.Success(prepared.Value.Calculation) : Result.Failure<LeaveCalculation>(prepared.Error);
    }, cancellationToken);

    public Task<Result<LeaveWriteResult>> SaveAsync(LeaveRequestInput input, bool personnelOffice,
        CancellationToken cancellationToken = default) => RunAsync<LeaveWriteResult>("Save", async db =>
    {
        var ct = cancellationToken;
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var target = await ResolveTargetAsync(db, input, personnelOffice, ct);
        if (target.IsFailure) return target.Error;
        var (person, existing) = target.Value;
        var type = input.Type.GetValueOrDefault();
        var needsSubstitute = type is not (LeaveType.Personal or LeaveType.Nursing);
        var needsManager = type != LeaveType.Nursing;
        var prepared = await PrepareAsync(db, person, input, existing?.OnayDurumu == Status.DilekceBekleniyor, ct);
        if (prepared.IsFailure) return prepared.Error;

        if ((needsSubstitute && !await IsValidSignerAsync(db, input.SubstituteId, existing?.VekilImza, true, ct)) ||
            (needsManager && !await IsValidSignerAsync(db, input.ManagerId, existing?.AmirImza, false, ct)))
            return Error.Validation(needsSubstitute ? "Geçerli bir vekil ve amir seçiniz." : "Geçerli bir amir seçiniz.");

        if (await db.IzinTalep_Table.AnyAsync(x => x.PersonelId == person.Id && x.IzinTipi == (int)type &&
            x.Id != input.Id && x.OnayDurumu != null && x.OnayDurumu != Status.KayitlaraIslendi &&
            x.OnayDurumu != Status.Reddedildi && x.OnayDurumu != Status.IptalEdildi, ct))
            return Error.Conflict("Bu izin türünde işlemi devam eden bir talep bulunuyor.");

        db.SetCurrentUser(await authorization.GetUserNameAsync(ct));
        var leave = prepared.Value;
        var request = existing ?? new IzinTalep { PersonelId = person.Id, IzinTipi = (int)type, Aktif = true, OnayDurumu = Status.IslemBekliyor };
        if (existing is null) db.IzinTalep_Table.Add(request);
        if (leave.Period is { } period)
        {
            if (period.Id == 0)
            {
                period.Aciklama = "Otomatik oluşturuldu.";
                db.IzinDonem_Table.Add(period);
            }
            period.BaslangicTarihi = leave.PeriodStart;
            period.BitisTarihi = leave.PeriodEnd;
            period.Adi = $"{leave.PeriodStart:yyyy}-{leave.PeriodEnd:yyyy} İzin Dönemi";
            period.Birim = leave.Calculation.Unit;
            period.IzinHakki = leave.Allowance;
            period.KalanIzin = leave.PeriodBalance;
            request.IzinDonem = period;
        }
        request.BaslangicTarihi = leave.Start;
        request.BitisTarihi = leave.End;
        request.Sure = leave.Calculation.Duration;
        request.Birim = leave.Calculation.Unit;
        request.VekilImza = needsSubstitute ? input.SubstituteId : 0;
        request.AmirImza = needsManager ? input.ManagerId : existing?.AmirImza ?? 0;
        request.OnayImza ??= 0;
        request.Adres = input.Address?.Trim();
        request.Aciklama = input.Description?.Trim();
        if (personnelOffice) request.EPostaGonder = input.Notify;
        else if (existing is null) request.EPostaGonder = true;
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        var failed = existing is null && request.EPostaGonder == true && !await NotifyAsync(request, person, null);
        return new LeaveWriteResult(request.Id, failed);
    }, cancellationToken);

    public async Task<Result> DeleteAsync(int id, bool personnelOffice, CancellationToken cancellationToken = default) =>
        await RunAsync<bool>("Delete", async db =>
        {
            var ct = cancellationToken;
            var actor = await FindActorAsync(db, personnelOffice, Operation.Delete, ct);
            if (actor.IsFailure) return actor.Error;
            await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
            var request = await FindRequestAsync(db, id, actor.Value, personnelOffice, ct);
            if (request is null) return MissingRequest;
            if (!CanEdit(request) || await HasMovementAsync(db, id, ct)) return ChangedRequest;
            db.IzinTalep_Table.Remove(request);
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return true;
        }, cancellationToken);

    private async Task<Result<(Personel Person, IzinTalep? Request)>> ResolveTargetAsync(ApplicationDbContext db,
        LeaveRequestInput input, bool personnelOffice, CancellationToken ct)
    {
        var actor = await FindActorAsync(db, personnelOffice, input.Id == 0 ? Operation.Create : Operation.Edit, ct);
        if (actor.IsFailure) return actor.Error;
        if (input.Id != 0)
        {
            var request = await FindRequestAsync(db, input.Id, actor.Value, personnelOffice, ct);
            if (request?.Personel is null) return MissingRequest;
            if (!CanEdit(request) || await HasMovementAsync(db, request.Id, ct)) return ChangedRequest;
            if (request.PersonelId != input.PersonelId || request.IzinTipi != (int?)input.Type)
                return Error.Validation("Mevcut talebin personeli veya izin türü değiştirilemez.");
            return (request.Personel, request);
        }
        if (!personnelOffice && input.PersonelId == actor.Value.Id) return (actor.Value, null);
        if (!personnelOffice) return AccessDenied;
        var person = await ActiveStaff(db).FirstOrDefaultAsync(x => x.Id == input.PersonelId, ct);
        return person is null ? Result.Failure<(Personel, IzinTalep?)>(Error.Validation("Çalışan kadrolu personel seçiniz.")) : (person, null);
    }

    private async Task<Result<Personel>> FindActorAsync(ApplicationDbContext db, bool personnelOffice, Operation operation, CancellationToken ct)
    {
        var requiredOperation = personnelOffice ? Operation.Manage : operation;
        if (!await authorization.HasPermissionAsync(ModuleName.IKYS, Operation.View, ct) ||
            (requiredOperation != Operation.View && !await authorization.HasPermissionAsync(ModuleName.IKYS, requiredOperation, ct)))
            return AccessDenied;
        var login = (await authorization.GetUserNameAsync(ct))?.Split('\\').Last().Split('@')[0].Trim();
        if (string.IsNullOrWhiteSpace(login)) return AccessDenied;
        var person = await db.Personel_Table.AsNoTracking().FirstOrDefaultAsync(x => x.KullaniciAdi == login, ct);
        return person is null ? Result.Failure<Personel>(AccessDenied) : person;
    }

    private static Task<IzinTalep?> FindRequestAsync(ApplicationDbContext db, int id, Personel actor,
        bool personnelOffice, CancellationToken ct) => db.IzinTalep_Table.Include(x => x.Personel)
        .FirstOrDefaultAsync(x => x.Id == id && x.Aktif == true && (personnelOffice || x.PersonelId == actor.Id), ct);

    private static IQueryable<LeaveRequestRow> Rows(ApplicationDbContext db, IQueryable<IzinTalep> requests) =>
        from request in requests
        join type in db.IzinTanim_Table on request.IzinTipi equals (int?)type.Id into types
        from type in types.DefaultIfEmpty()
        select new LeaveRequestRow(request.Id, request.Personel == null ? "" : request.Personel.Adi + " " + request.Personel.Soyadi,
            type.Adi ?? "İzin", request.IzinTipi, request.BaslangicTarihi, request.BitisTarihi, request.Sure, request.Birim, request.OnayDurumu);

    private static bool CanEdit(IzinTalep request) => request.OnayDurumu is Status.IslemBekliyor or Status.DilekceBekleniyor;

    private static Task<bool> HasMovementAsync(ApplicationDbContext db, int id, CancellationToken ct) =>
        db.IzinHareket_Table.AnyAsync(x => x.IzinTalepId == id, ct);

    private static async Task<bool> IsValidSignerAsync(ApplicationDbContext db, int? id, int? previous, bool substitute, CancellationToken ct) =>
        id is > 0 && (!substitute || id != GeneralManagerId) &&
        (id == previous || await ActiveStaff(db).AnyAsync(x => x.Id == id, ct));

    private async Task<Result<T>> RunAsync<T>(string operation, Func<ApplicationDbContext, Task<Result<T>>> action, CancellationToken ct)
    {
        try
        {
            await using var db = await dbFactory.CreateDbContextAsync(ct);
            return await action(db);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            log.LogException(ex, $"LeaveRequestWorkflow.{operation}");
            return Error.Unexpected("İzin talebi işlemi tamamlanamadı. Listeyi yenileyip tekrar deneyiniz.", ex);
        }
    }
}
