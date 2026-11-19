using Jenga.DataAccess.Data;
using Jenga.Models.IKYS;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.IKYS;

public sealed partial class LeaveRequestWorkflow
{
    private static async Task<LeaveDocumentInfo> GetDocumentInfoAsync(ApplicationDbContext db, IzinTalep request,
        List<IzinDonem> periods, CancellationToken ct)
    {
        var people = new[] { request.PersonelId, request.VekilImza, request.AmirImza }
            .Where(id => id is > 0).Select(id => id!.Value).Distinct().ToArray();
        var jobs = await db.IsBilgileri_Table.AsNoTracking()
            .Where(job => people.Contains(job.PersonelId))
            .OrderBy(job => job.UnvanId).ThenBy(job => job.Id)
            .Select(job => new
            {
                job.PersonelId, job.IzinDonemiBasTar, job.VakifOncesiPrimGunSayisi,
                Title = job.UnvanTanim == null ? null : job.UnvanTanim.Adi,
                Department = job.BirimTanim == null ? null : job.BirimTanim.Adi
            }).ToListAsync(ct);
        var employee = jobs.Find(job => job.PersonelId == request.PersonelId);
        var substitute = jobs.Find(job => job.PersonelId == request.VekilImza);
        var manager = jobs.Find(job => job.PersonelId == request.AmirImza);

        DateTime? periodStart = null;
        DateTime? periodEnd = null;
        string? entitlement = null;
        var usedDays = new Dictionary<LeaveType, int>();
        if (request.IzinTipi != (int)LeaveType.Personal && request.BaslangicTarihi is { } start)
        {
            var annual = periods.Find(period => period.IzinTipi == (int)LeaveType.Paid &&
                period.BaslangicTarihi?.Date <= start.Date && period.BitisTarihi?.Date >= start.Date);
            periodStart = annual?.BaslangicTarihi?.Date;
            periodEnd = annual?.BitisTarihi?.Date;
            entitlement = annual?.IzinHakki;

            if (annual is null && employee?.IzinDonemiBasTar is { } entitlementStart &&
                entitlementStart >= FirstLeaveDate && start.Year is > 1 and < 9999 && request.Personel is { } person)
            {
                var anniversary = Anniversary(start.Year, entitlementStart);
                if (anniversary > start.Date) anniversary = Anniversary(start.Year - 1, entitlementStart);
                periodStart = anniversary;
                periodEnd = Anniversary(anniversary.Year + 1, entitlementStart).AddDays(-1);
                entitlement = Number(PaidAllowance(person, anniversary.Year - entitlementStart.Year, employee.VakifOncesiPrimGunSayisi));
            }

            if (periodStart is { } from && periodEnd is { Year: < 9999 } through)
            {
                var to = through.AddDays(1);
                var movements = await db.IzinHareket_Table.AsNoTracking()
                    .Where(movement => movement.PersonelId == request.PersonelId && movement.Mahsup != true &&
                        movement.IzinTalepId != request.Id && movement.BaslangicTarihi >= from && movement.BaslangicTarihi < to &&
                        movement.IzinTipi != (int)LeaveType.Personal && movement.IzinTipi != (int)LeaveType.Nursing &&
                        movement.IzinTipi != null)
                    .Select(movement => new { movement.IzinTipi, movement.Sure }).ToListAsync(ct);
                usedDays = movements.GroupBy(movement => (LeaveType)movement.IzinTipi!.Value)
                    .ToDictionary(group => group.Key, group => group.Sum(movement => Days(movement.Sure)));
            }
        }

        return new LeaveDocumentInfo(request.OlusturmaTarihi, request.Personel?.SicilNo, employee?.Title,
            employee?.Department, substitute?.Title, manager?.Title, periodStart, periodEnd, entitlement, usedDays);
    }
}
