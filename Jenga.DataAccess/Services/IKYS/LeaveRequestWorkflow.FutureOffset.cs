using Jenga.DataAccess.Data;
using Jenga.Models.IKYS;
using Microsoft.EntityFrameworkCore;
using Status = Jenga.Models.IKYS.IzinTalep.OnayDurumuTipi;

namespace Jenga.DataAccess.Services.IKYS;

public sealed partial class LeaveRequestWorkflow
{
    private static bool IsFuturePeriodOffset(IzinTalep request) =>
        request.IzinTipi == (int)LeaveType.Paid &&
        (request.GelecekDonemMahsubu == true || request.OnayDurumu == Status.DilekceBekleniyor);

    private static int AvailableDays(IzinDonem period) =>
        Math.Min(Days(period.KalanIzin), Days(period.IzinHakki) - Days(period.KullanilanIzin));

    private static FutureLeavePeriod PeriodOption(IzinDonem period) =>
        new(period.Id, period.BaslangicTarihi!.Value.Date, period.BitisTarihi!.Value.Date, AvailableDays(period));

    private static async Task<FutureLeaveOffset> GetFutureOffsetAsync(ApplicationDbContext db, IzinTalep request,
        IzinDonem? source, bool personnelOffice, CancellationToken ct)
    {
        List<FutureLeavePeriod> targets = [];
        FutureLeavePeriod? applied = null;
        if (source is not null && request.Personel is { } person)
        {
            if (personnelOffice && request.OnayDurumu == Status.KontrolEdildi)
            {
                var eligible = await FutureOffsetTargetsAsync(db, person, source, Days(request.Sure), ct);
                targets = eligible.Select(PeriodOption).ToList();
            }
            else if (request.OnayDurumu == Status.KayitlaraIslendi)
            {
                var target = await (
                    from offset in db.Mahsup_Table.AsNoTracking()
                    join movement in db.IzinHareket_Table on offset.IzinHareketId equals movement.Id
                    join period in db.IzinDonem_Table on offset.MahsupDonemId equals period.Id
                    where movement.IzinTalepId == request.Id && offset.PersonelId == person.Id &&
                          offset.IzinTipi == (int)LeaveType.Paid && offset.KullanildigiDonemId == source.Id &&
                          period.PersonelId == person.Id && period.BaslangicTarihi != null && period.BitisTarihi != null
                    orderby offset.Id descending
                    select period).AsNoTracking().FirstOrDefaultAsync(ct);
                if (target is not null) applied = PeriodOption(target);
            }
        }
        return new FutureLeaveOffset(source?.BaslangicTarihi, source?.BitisTarihi, targets, applied);
    }

    private static async Task<List<IzinDonem>> FutureOffsetTargetsAsync(ApplicationDbContext db, Personel person,
        IzinDonem source, int requestedDays, CancellationToken ct)
    {
        if (requestedDays <= 0 || source.BitisTarihi is not { } end || end.Year >= 9999) return [];

        var periods = await db.IzinDonem_Table.Where(x => x.PersonelId == person.Id && x.IzinTipi == (int)LeaveType.Paid &&
            x.BaslangicTarihi != null && x.BitisTarihi >= end.Date.AddDays(1)).ToListAsync(ct);
        var eligible = periods.Where(x => x.BaslangicTarihi?.Date > end.Date &&
            x.BaslangicTarihi?.Date <= x.BitisTarihi?.Date && AvailableDays(x) >= requestedDays).ToList();

        var nextStart = end.Date.AddDays(1);
        var job = await FindJobAsync(db, person.Id, ct);
        if (nextStart.Year < 9999 && job?.IzinDonemiBasTar is { } entitlementStart && entitlementStart >= FirstLeaveDate)
        {
            var nextEnd = Anniversary(nextStart.Year + 1, entitlementStart).AddDays(-1);
            var allowance = PaidAllowance(person, nextStart.Year - entitlementStart.Year, job.VakifOncesiPrimGunSayisi);
            if (allowance >= requestedDays && !periods.Any(x => x.BaslangicTarihi?.Date <= nextEnd))
            {
                eligible.Add(new IzinDonem
                {
                    PersonelId = person.Id, IzinTipi = (int)LeaveType.Paid, Birim = "Gün",
                    BaslangicTarihi = nextStart, BitisTarihi = nextEnd,
                    Adi = $"{nextStart:yyyy}-{nextEnd:yyyy} İzin Dönemi",
                    IzinHakki = Number(allowance), KullanilanIzin = "0", KalanIzin = Number(allowance),
                    Aciklama = "Yıllık izin mahsubu için oluşturuldu."
                });
            }
        }
        return eligible.OrderBy(x => x.BaslangicTarihi).ThenBy(x => x.Id).ToList();
    }
}
