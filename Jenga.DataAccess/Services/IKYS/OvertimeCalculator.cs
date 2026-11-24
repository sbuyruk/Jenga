using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Services.IKYS;

public static class OvertimeCalculator
{
    public static DateTime MondayOf(DateTime date) => date.Date.AddDays(-((int)date.DayOfWeek + 6) % 7);
    public static int RestMinutes(int grossMinutes) => grossMinutes <= 0 ? 0 : grossMinutes <= 240 ? 15 : grossMinutes <= 450 ? 30 : 60;
    public static int NetMinutes(DateTime day, int grossMinutes, IReadOnlySet<DateTime>? publicHolidays = null)
        => grossMinutes - (day.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday || publicHolidays?.Contains(day.Date) == true ? RestMinutes(grossMinutes) : 0);
    public static int RoundTotal(int minutes) => checked((minutes + 29) / 30 * 30);

    private static int RestEligibleMinutes(DateTime day, int startMinute, int endMinute, IReadOnlySet<DateTime>? holidays, IReadOnlyDictionary<DateTime, int>? holidayStarts)
    {
        if (day.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday) return endMinute - startMinute;
        if (holidayStarts?.TryGetValue(day.Date, out var from) == true) return Math.Max(0, endMinute - Math.Max(startMinute, from));
        return holidays?.Contains(day.Date) == true ? endMinute - startMinute : 0;
    }

    public static int NetMinutes(DateTime day, IEnumerable<OvertimeInterval> intervals, IReadOnlySet<DateTime>? holidays = null, IReadOnlyDictionary<DateTime, int>? holidayStarts = null)
    {
        var rows = intervals.ToList();
        return rows.Sum(x => x.EndMinute - x.StartMinute) - RestMinutes(rows.Sum(x => RestEligibleMinutes(day, x.StartMinute, x.EndMinute, holidays, holidayStarts)));
    }

    public static List<OvertimeWeek> Calculate(IEnumerable<OvertimeEntry> entries, DateTime start, DateTime end, IReadOnlySet<DateTime>? publicHolidays = null, IReadOnlyDictionary<DateTime, int>? holidayStarts = null)
    {
        if (start.DayOfWeek != DayOfWeek.Monday || end.DayOfWeek != DayOfWeek.Sunday || end.Date < start.Date)
            throw new ArgumentException("Dönem, Pazartesi başlayıp Pazar biten tam haftalardan oluşmalıdır.");

        var source = entries.ToList();
        if (source.Select(x => x.PersonelId).Distinct().Count() > 1)
            throw new ArgumentException("Hesaplama tek personel için yapılmalıdır.");
        var days = new Dictionary<DateTime, (int Gross, int Normal, int RestEligible)>();
        foreach (var entry in source)
        {
            if (entry.NormalWeeklyMinutes is not (2400 or 2700))
                throw new ArgumentException("Personelin haftalık çalışma grubu 40 veya 45 saat olmalıdır.");
            foreach (var interval in entry.Intervals)
            {
                var from = entry.WorkDate.Date.AddMinutes(interval.StartMinute);
                var until = entry.WorkDate.Date.AddMinutes(interval.EndMinute);
                while (from < until)
                {
                    var next = until < from.Date.AddDays(1) ? until : from.Date.AddDays(1);
                    if (from.Date < start.Date || from.Date > end.Date)
                        throw new ArgumentException("Çalışma aralığı dönem sınırını aşıyor; sınırdaki kayıt günlere ayrılmalıdır.");
                    var old = days.GetValueOrDefault(from.Date);
                    if (old.Normal != 0 && old.Normal != entry.NormalWeeklyMinutes)
                        throw new ArgumentException("Aynı gün için farklı çalışma grupları bulunuyor.");
                    var eligible = RestEligibleMinutes(from.Date, (int)from.TimeOfDay.TotalMinutes, (int)(next - from.Date).TotalMinutes, publicHolidays, holidayStarts);
                    days[from.Date] = (old.Gross + (int)(next - from).TotalMinutes, entry.NormalWeeklyMinutes, old.RestEligible + eligible);
                    from = next;
                }
            }
        }

        var weeks = new List<OvertimeWeek>();
        foreach (var week in days.GroupBy(x => MondayOf(x.Key)).OrderBy(x => x.Key))
        {
            var groups = week.Select(x => x.Value.Normal).Distinct().ToList();
            if (groups.Count != 1)
                throw new ArgumentException("Aynı haftada farklı çalışma grupları var; İK kontrolü gerekiyor.");
            var gross = week.Sum(x => x.Value.Gross);
            var rest = week.Sum(x => RestMinutes(x.Value.RestEligible));
            weeks.Add(new(week.Key, groups[0], gross, rest, Math.Max(0, groups[0] + gross - rest - 2700)));
        }
        return weeks;
    }
}
