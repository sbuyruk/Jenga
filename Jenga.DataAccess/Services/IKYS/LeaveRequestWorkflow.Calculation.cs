using System.Globalization;
using Jenga.DataAccess.Data;
using Jenga.Models.Enums;
using Jenga.Models.IKYS;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.IKYS;

public sealed partial class LeaveRequestWorkflow
{
    private static readonly DateTime FirstLeaveDate = new(1987, 1, 1);
    private static readonly TimeOnly WorkStart = new(8, 0);
    private static readonly TimeOnly WorkEnd = new(17, 0);
    private static readonly TimeOnly LunchStart = new(12, 0);
    private static readonly TimeOnly LunchEnd = new(13, 0);
    private const int GeneralManagerId = 1184;
    private const int ChildRelationshipId = 2;

    private sealed record PreparedLeave(DateTime Start, DateTime End, LeaveCalculation Calculation,
        IzinDonem? Period = null, DateTime? PeriodStart = null, DateTime? PeriodEnd = null,
        string? Allowance = null, string? PeriodBalance = null);

    private sealed record PendingLeave(string? Duration, int? PeriodId, DateTime? Start);

    private static IQueryable<Personel> WorkingPersonnel(ApplicationDbContext db) => db.Personel_Table
        .Where(person => db.IsBilgileri_Table.Any(
            job => job.PersonelId == person.Id && job.CalismaDurumu == CalismaDurumu.Calisiyor));

    private static IQueryable<Personel> ActiveStaff(ApplicationDbContext db) =>
        WorkingPersonnel(db).Where(person => person.Tipi == PersonelTipi.Kadrolu);

    private static Task<IsBilgileri?> FindJobAsync(ApplicationDbContext db, int personId, CancellationToken ct) =>
        db.IsBilgileri_Table.Where(x => x.PersonelId == personId)
            .OrderBy(x => x.UnvanId).ThenBy(x => x.Id).FirstOrDefaultAsync(ct);

    private static bool UsesCalendarDays(LeaveType type) =>
        type is LeaveType.Paternity or LeaveType.Marriage or LeaveType.Bereavement;

    private static bool HasBalance(LeaveType type) => type is LeaveType.Paid or LeaveType.Personal;

    private static bool ValidTime(TimeOnly time) =>
        time >= WorkStart && time <= WorkEnd && time.Ticks % TimeSpan.FromMinutes(5).Ticks == 0;

    private static int CalendarDays(DateTime start, DateTime end) => (end.Date - start.Date).Days + 1;

    private static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);

    private static int Days(string? value) => string.IsNullOrWhiteSpace(value) ? 0 : int.Parse(value, CultureInfo.InvariantCulture);

    private static TimeSpan Hours(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value == "0") return TimeSpan.Zero;
        return TimeSpan.Parse(value, CultureInfo.InvariantCulture);
    }

    private static string DisplayHours(TimeSpan value)
    {
        var absolute = value.Duration();
        return $"{(value < TimeSpan.Zero ? "-" : "")}{(int)absolute.TotalHours:00}:{absolute.Minutes:00}";
    }

    private static async Task<Result<PreparedLeave>> PrepareAsync(ApplicationDbContext db, Personel person,
        LeaveRequestInput input, CancellationToken ct, bool includePending = false)
    {
        if (input.Type is not { } type || !Enum.IsDefined(type) ||
            !await db.IzinTanim_Table.AnyAsync(x => x.Id == (int)type, ct))
            return Error.Validation("Geçerli bir izin türü seçiniz.");

        if (input.StartDate is not { } start || (type != LeaveType.Personal && input.EndDate is null))
            return Error.Validation("İzin tarihlerini giriniz.");

        start = start.Date;
        var end = type == LeaveType.Personal ? start : input.EndDate!.Value.Date;
        if (start < FirstLeaveDate || end < start || end.Year >= 9999)
            return Error.Validation("Geçerli bir izin tarih aralığı giriniz.");

        var job = await FindJobAsync(db, person.Id, ct);
        if (job?.IzinDonemiBasTar is not { } entitlementStart || entitlementStart < FirstLeaveDate)
            return Error.Validation("Personelin iş bilgileri veya izin dönemi başlangıcı eksik.");

        var unit = type == LeaveType.Personal ? "Saat" : "Gün";
        string duration;
        if (type == LeaveType.Personal)
        {
            if (input.StartTime is not { } from || input.EndTime is not { } to ||
                !ValidTime(from) || !ValidTime(to) || from >= to ||
                (from >= LunchStart && from < LunchEnd) || (to > LunchStart && to <= LunchEnd))
                return Error.Validation("Mazeret izni için 08:00–17:00 arasında, beş dakikalık aralıklarla geçerli saatler seçiniz.");

            var used = to - from;
            if (from <= LunchStart && to >= LunchEnd) used -= LunchEnd - LunchStart;
            start = start.Add(from.ToTimeSpan());
            end = end.Add(to.ToTimeSpan());
            duration = used.ToString("c", CultureInfo.InvariantCulture);
        }
        else if (type == LeaveType.Nursing)
        {
            if (input.StartTime is not { } from || !ValidTime(from) ||
                from > WorkEnd.AddMinutes(-90) || (from >= LunchStart && from < LunchEnd))
                return Error.Validation("Süt izni için çalışma saatleri içinde günlük 90 dakikaya uygun bir başlangıç saati seçiniz.");
            var youngest = await db.Aile_Table.Where(x => x.PersonelId == person.Id && x.YakinlikDerecesi == ChildRelationshipId)
                .OrderByDescending(x => x.DogumTar).Select(x => x.DogumTar).FirstOrDefaultAsync(ct);
            if (youngest is null || start < youngest.Value.Date || end > youngest.Value.Date.AddYears(1))
                return Error.Validation("Süt izni tarihleri en küçük çocuğun doğum tarihi ile ilk yaş günü arasında olmalıdır.");
            start = start.Add(from.ToTimeSpan());
            end = end.Add(from.ToTimeSpan()).AddMinutes(90);
            duration = Number(CalendarDays(start, end));
        }
        else
        {
            var days = type == LeaveType.Paid ? await CountPaidDaysAsync(db, start, end, ct) : CalendarDays(start, end);
            if (days == 0)
                return Error.Validation("Seçilen aralıkta ücretli izinden düşülecek iş günü bulunmuyor.");
            duration = Number(days);
            end = end.AddHours(17);
        }

        var pending = includePending ? await PendingRequests(db, person.Id, type, input.Id)
            .Select(x => new PendingLeave(x.Sure, x.IzinDonemId, x.BaslangicTarihi)).ToListAsync(ct) : [];

        if (!HasBalance(type))
        {
            var pendingDays = pending.Sum(x => Math.Max(0, Days(x.Duration)));
            var calculation = pendingDays > 0
                ? new LeaveCalculation(duration, unit, null, null, PendingDuration: Number(pendingDays),
                    CombinedDuration: Number(Days(duration) + pendingDays))
                : new LeaveCalculation(duration, unit, null, null);
            return new PreparedLeave(start, end, calculation);
        }

        var periods = await db.IzinDonem_Table.Where(x => x.PersonelId == person.Id && x.IzinTipi == (int)type)
            .OrderBy(x => x.BaslangicTarihi).ThenBy(x => x.Id).ToListAsync(ct);
        var period = periods.FirstOrDefault(x => x.BaslangicTarihi?.Date <= start.Date && x.BitisTarihi?.Date >= start.Date);
        var periodStart = type == LeaveType.Personal ? new DateTime(start.Year, 1, 1) : Anniversary(start.Year, entitlementStart);
        if (periodStart > start) periodStart = Anniversary(start.Year - 1, entitlementStart);
        var periodEnd = (type == LeaveType.Personal ? periodStart.AddYears(1) : Anniversary(periodStart.Year + 1, entitlementStart)).AddDays(-1);
        period ??= new IzinDonem { PersonelId = person.Id, IzinTipi = (int)type, KullanilanIzin = "0" };

        if (type == LeaveType.Personal)
        {
            var allowance = period.Id > 0 ? Hours(period.IzinHakki) : TimeSpan.FromHours(27);
            var balance = allowance - Hours(period.KullanilanIzin);
            var pendingHours = TimeSpan.FromTicks(pending
                .Where(x => (period.Id > 0 && x.PeriodId == period.Id) ||
                    (x.PeriodId.GetValueOrDefault() <= 0 && x.Start?.Date >= periodStart && x.Start?.Date <= periodEnd))
                .Sum(x => Math.Max(0, Hours(x.Duration).Ticks)));
            var availableAfterPending = balance - pendingHours;
            var remaining = availableAfterPending - Hours(duration);
            if (remaining < TimeSpan.Zero && pendingHours == TimeSpan.Zero)
                return Error.Validation($"Mazeret izni bakiyesi yetersiz. Kullanılabilir süre: {DisplayHours(balance)} saat.");

            var calculation = new LeaveCalculation(duration, unit, DisplayHours(balance), DisplayHours(remaining),
                PendingDuration: pendingHours > TimeSpan.Zero ? DisplayHours(pendingHours) : null,
                AvailableAfterPending: pendingHours > TimeSpan.Zero ? DisplayHours(availableAfterPending) : null,
                BalanceWarning: remaining < TimeSpan.Zero ? "Bekleyen talepler hesaba katıldığında mazeret izni bakiyesi yetersiz." : null);
            return new PreparedLeave(start, end, calculation, period, periodStart, periodEnd, allowance.ToString("c"), balance.ToString("c"));
        }

        var serviceYears = periodStart.Year - entitlementStart.Year;
        var annualAllowance = PaidAllowance(person, serviceYears, job.VakifOncesiPrimGunSayisi);
        var periodBalance = annualAllowance - Days(period.KullanilanIzin);
        var previousPeriodBalance = periods.Where(x => x.Id != period.Id && x.BitisTarihi?.Date < periodStart)
            .Sum(x => Days(x.KalanIzin));
        var available = periodBalance + previousPeriodBalance;
        var reservedDays = pending.Sum(x => Math.Max(0, Days(x.Duration)));
        var availableDaysAfterPending = available - reservedDays;
        var afterRequest = availableDaysAfterPending - Days(duration);
        var paidCalculation = new LeaveCalculation(duration, unit, Number(available), Number(afterRequest),
            PendingDuration: reservedDays > 0 ? Number(reservedDays) : null,
            AvailableAfterPending: reservedDays > 0 ? Number(availableDaysAfterPending) : null,
            BalanceWarning: afterRequest < 0
                ? reservedDays > 0 ? "Bekleyen talepler hesaba katıldığında yıllık izin bakiyesi yetersiz."
                    : "Mevcut yıllık izin bakiyesi talep edilen süreyi karşılamıyor."
                : null,
            PreviousPeriodBalance: previousPeriodBalance,
            CanRequestFuturePeriodOffset: available < Days(duration) && pending.Count == 0);
        return new PreparedLeave(start, end, paidCalculation, period, periodStart, periodEnd, Number(annualAllowance), Number(periodBalance));
    }

    private static DateTime Anniversary(int year, DateTime entitlementStart) =>
        new(year, entitlementStart.Month, Math.Min(entitlementStart.Day, DateTime.DaysInMonth(year, entitlementStart.Month)));

    private static int PaidAllowance(Personel person, int serviceYears, int priorPremiumDays)
    {
        if (serviceYears < 1) return 0;
        if (person.AskerSivil == AskerSivil.Asker) return 26;
        var totalYears = serviceYears + Math.Max(0, priorPremiumDays) / 360;
        return totalYears >= 15 ? 26 : totalYears > 5 ? 20 : 14;
    }

    private static async Task<int> CountPaidDaysAsync(ApplicationDbContext db, DateTime start, DateTime end, CancellationToken ct) =>
        (await PaidWorkdaysAsync(db, start, end, ct)).Count();

    private static async Task<IEnumerable<DateTime>> PaidWorkdaysAsync(ApplicationDbContext db, DateTime start, DateTime end, CancellationToken ct)
    {
        var holidays = await db.ResmiTatil_Table.AsNoTracking()
            .Where(x => x.Yil == 0 || (x.BaslamaTarihi <= end && x.BitisTarihi >= start)).ToListAsync(ct);
        return Workdays();

        IEnumerable<DateTime> Workdays()
        {
            for (var date = start; date <= end; date = date.AddDays(1))
            {
                if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday) continue;
                if (holidays.Any(x => (x.BaslamaTarihi <= date && x.BitisTarihi >= date) ||
                    (x.Yil == 0 && x.Ay == date.Month && x.BaslamaTarihi.Day <= date.Day && x.BitisTarihi.Day >= date.Day &&
                     (x.IlanTarihi is null || x.IlanTarihi <= date) && (x.IptalTarihi is null || x.IptalTarihi >= date)))) continue;
                yield return date;
            }
        }
    }
}
