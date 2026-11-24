using Jenga.DataAccess.Data;
using Jenga.Models.IKYS;
using Jenga.Utility.Results;
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

    private static int OffsetDays(int requested, int available) => requested - Math.Clamp(available, 0, requested);

    private static async Task<int> PaidBalanceAsync(ApplicationDbContext db, IzinDonem source, CancellationToken ct)
    {
        var priorBalances = await db.IzinDonem_Table.Where(x => x.Id != source.Id && x.PersonelId == source.PersonelId &&
            x.IzinTipi == (int)LeaveType.Paid && x.BitisTarihi < source.BaslangicTarihi!.Value.Date)
            .Select(x => x.KalanIzin).ToListAsync(ct);
        return Days(source.IzinHakki) - Days(source.KullanilanIzin) + priorBalances.Sum(Days);
    }

    private static FutureLeavePeriod PeriodOption(IzinDonem period) =>
        new(period.Id, period.BaslangicTarihi!.Value.Date, period.BitisTarihi!.Value.Date, AvailableDays(period));

    private static async Task<Result> ApplyFutureOffsetAsync(ApplicationDbContext db, IzinTalep request,
        IzinDonem source, IzinDonem target, DateTime start, DateTime end, CancellationToken ct)
    {
        var offsetDays = request.GelecekDonemMahsupGun!.Value;
        var localDays = Days(request.Sure) - offsetDays;
        var offsetStart = start;
        if (localDays > 0)
        {
            var workdays = (await PaidWorkdaysAsync(db, start, end, ct)).ToArray();
            if (workdays.Length != localDays + offsetDays)
                return Result.Failure(Error.Validation("İş günü hesabı değişti. Mahsup dilekçesini güncel tarihlerle yeniden oluşturunuz."));

            var current = CreateMovement(request, start, workdays[localDays - 1].Date + end.TimeOfDay);
            ApplyPaidDays(source, current, localDays);
            db.IzinHareket_Table.Add(current);
            offsetStart = workdays[localDays];
        }

        if (target.Id == 0) db.IzinDonem_Table.Add(target);
        var offset = CreateMovement(request, offsetStart, end);
        offset.Mahsup = true;
        offset.IzinDonemId = null;
        offset.IzinDonem = target;
        ApplyPaidDays(target, offset, offsetDays, useStoredBalance: true);
        db.Mahsup_Table.Add(new Mahsup
        {
            PersonelId = request.PersonelId!.Value, IzinTipi = (int)LeaveType.Paid, IzinHareket = offset, MahsupDonem = target,
            KullanildigiDonemId = source.Id, Aciklama = "Yıllık izin gelecek dönemden mahsup edildi."
        });
        return Result.Success();
    }

    private static void ApplyPaidDays(IzinDonem period, IzinHareket movement, int duration, bool useStoredBalance = false)
    {
        var used = Days(period.KullanilanIzin);
        var available = useStoredBalance ? Days(period.KalanIzin) : Days(period.IzinHakki) - used;
        movement.OncekiIzinStr = period.KalanIzin;
        movement.Sure = movement.KullanilanIzinStr = Number(duration);
        period.KullanilanIzin = Number(used + duration);
        movement.KalanIzinStr = period.KalanIzin = Number(available - duration);
    }

    private static async Task<FutureLeaveOffset> GetFutureOffsetAsync(ApplicationDbContext db, IzinTalep request,
        IzinDonem? source, bool personnelOffice, CancellationToken ct)
    {
        List<FutureLeavePeriod> targets = [];
        FutureLeavePeriod? applied = null;
        var requested = Math.Max(0, Days(request.Sure));
        var offsetDays = request.GelecekDonemMahsupGun ?? requested;
        if (source?.BaslangicTarihi is not null && request.Personel is { } person)
        {
            if (request.GelecekDonemMahsupGun is null && request.OnayDurumu is (Status.DilekceBekleniyor or Status.KontrolEdildi))
                offsetDays = OffsetDays(requested, await PaidBalanceAsync(db, source, ct));
            if (personnelOffice && request.OnayDurumu == Status.KontrolEdildi)
            {
                var eligible = await FutureOffsetTargetsAsync(db, person, source, offsetDays, ct);
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
                    select new { Period = period, movement.Sure }).AsNoTracking().FirstOrDefaultAsync(ct);
                if (target is not null)
                {
                    applied = PeriodOption(target.Period);
                    offsetDays = Days(target.Sure);
                }
            }
        }
        return new FutureLeaveOffset(source?.BaslangicTarihi, source?.BitisTarihi, targets, applied, offsetDays);
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
