using Jenga.DataAccess.Data;
using Jenga.DataAccess.Services.Common;
using Jenga.Models.Enums;
using Jenga.Models.IKYS;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.IKYS;

public sealed class PersonelSummaryService(
    IDbContextFactory<ApplicationDbContext> factory,
    IServiceAuthorizationContext authorization,
    ILogService log) : IPersonelSummaryService
{
    public async Task<Result<PersonelSummary>> GetAsync(int personelId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await authorization.HasPermissionAsync(ModuleName.IKYS, Operation.View, cancellationToken))
                return Result.Failure<PersonelSummary>(Error.Forbidden("Personel özetini görüntüleme yetkiniz bulunmuyor."));
            var canManage = await authorization.HasPermissionAsync(ModuleName.IKYS, Operation.Manage, cancellationToken);
            var regions = canManage ? [] : (await authorization.GetAuthorizedRegionIdsAsync(cancellationToken)).ToArray();
            var userName = await authorization.GetUserNameAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(userName))
                return Result.Failure<PersonelSummary>(Error.Forbidden("Personel oturumu bulunamadı."));

            await using var db = await factory.CreateDbContextAsync(cancellationToken);
            var login = userName.Split('\\').Last().Split('@')[0];
            var actorId = await db.Personel_Table.Where(p => p.KullaniciAdi == login)
                .Select(p => (int?)p.Id).SingleOrDefaultAsync(cancellationToken);
            if (!actorId.HasValue)
                return Result.Failure<PersonelSummary>(Error.Forbidden("Oturum bir personel kaydıyla eşleştirilemedi."));

            var people = db.Personel_Table.AsNoTracking().Where(p => p.Id == personelId);
            if (!canManage)
                people = people.Where(p => db.IsBilgileri_Table.Any(i => i.PersonelId == p.Id &&
                    db.BirimTanim_Table.Any(b => b.Id == i.BirimId && b.BolgeId.HasValue && regions.Contains(b.BolgeId.Value))));

            var summary = await (from p in people
                join i in db.IsBilgileri_Table on p.Id equals i.PersonelId into infos
                from i in infos.DefaultIfEmpty()
                join b in db.BirimTanim_Table on i.BirimId equals b.Id into units
                from b in units.DefaultIfEmpty()
                join u in db.UnvanTanim_Table on i.UnvanId equals u.Id into titles
                from u in titles.DefaultIfEmpty()
                join g in db.GorevTanim_Table on i.GorevId equals g.Id into jobs
                from g in jobs.DefaultIfEmpty()
                join c in db.IletisimBilgileri_Table on p.Id equals c.PersonelId into contacts
                from c in contacts.DefaultIfEmpty()
                select new PersonelSummary
                {
                    PersonelId = p.Id, FullName = p.Adi + " " + p.Soyadi, SicilNo = p.SicilNo,
                    PersonnelType = p.Tipi, UnitName = b.Adi, Title = u.Adi, JobName = g.Adi,
                    StartDate = i.BaslamaTar, EmploymentStatus = i.CalismaDurumu,
                    OfficeExtension = c.DahiliTelefonu, WorkEmail = c.IntranetEPosta
                }).SingleOrDefaultAsync(cancellationToken);
            if (summary is null)
                return Result.Failure<PersonelSummary>(Error.NotFound("Personel bulunamadı veya bu kayda erişiminiz yok."));

            var tasks = await GorevOnayService.SearchableApprovals(db, actorId.Value, canManage)
                .Where(g => g.PersonelId == personelId)
                .OrderByDescending(g => g.BaslangicTarihi).ThenByDescending(g => g.Id).Take(5)
                .Select(g => new { g.Id, g.PersonelId, g.BaslangicTarihi, g.BitisTarihi, g.GorevinYeri, g.GorevinSebebi, g.AmirOnayi })
                .ToListAsync(cancellationToken);
            summary.TaskApprovals = tasks.Select(g => new PersonelSummaryActivity(g.Id,
                $"Görev Onayı #{g.Id}", g.BaslangicTarihi, g.GorevinSebebi,
                $"/ikys/task-approvals?SecilenId={g.Id}" + (g.PersonelId == actorId ? "" : "&Auth=IKYS"),
                g.AmirOnayi, g.BitisTarihi, g.GorevinYeri)).ToList();

            var access = await OvertimeService.SearchableEntriesAsync(db, actorId.Value, canManage, cancellationToken);
            var overtime = await access.Entries.Where(e => e.PersonelId == personelId && (canManage ||
                    db.BirimTanim_Table.Any(b => b.Id == e.UnitId && b.BolgeId.HasValue && regions.Contains(b.BolgeId.Value))))
                .OrderByDescending(e => e.WorkDate).ThenByDescending(e => e.Id).Take(5)
                .Select(e => new { e.Id, e.WorkDate, e.Reason }).ToListAsync(cancellationToken);
            var scope = personelId == actorId ? "mine" : canManage ? "hr" :
                access.DelegatedIds.Contains(personelId) ? "delegated" : "team";
            summary.OvertimeEntries = overtime.Select(e => new PersonelSummaryActivity(e.Id,
                $"Fazla Mesai #{e.Id}", e.WorkDate, e.Reason,
                $"/ikys/overtime?date={e.WorkDate:yyyy-MM-dd}&scope={scope}")).ToList();
            return Result.Success(summary);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            log.LogException(ex, nameof(PersonelSummaryService));
            return Result.Failure<PersonelSummary>(Error.Unexpected("Personel özeti yüklenemedi.", ex));
        }
    }
}
