using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Services.IKYS;

public static class OvertimeCalculator
{
    public static DateTime MondayOf(DateTime date) => date.Date.AddDays(-((int)date.DayOfWeek + 6) % 7);
    public static int RestMinutes(int grossMinutes) => grossMinutes <= 0 ? 0 : grossMinutes <= 240 ? 15 : grossMinutes <= 450 ? 30 : 60;
    public static int RoundTotal(int minutes) => checked((minutes + 29) / 30 * 30);

    public static List<OvertimeWeek> Calculate(IEnumerable<OvertimeEntry> entries, DateTime start, DateTime end)
    {
        if (start.DayOfWeek != DayOfWeek.Monday || end.DayOfWeek != DayOfWeek.Sunday || end.Date < start.Date)
            throw new ArgumentException("Dönem, Pazartesi başlayıp Pazar biten tam haftalardan oluşmalıdır.");

        var source = entries.ToList();
        if (source.Select(x => x.PersonelId).Distinct().Count() > 1)
            throw new ArgumentException("Hesaplama tek personel için yapılmalıdır.");
        var days = new Dictionary<DateTime, (int Gross, int Normal)>();
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
                    days[from.Date] = (old.Gross + (int)(next - from).TotalMinutes, entry.NormalWeeklyMinutes);
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
            var rest = week.Where(x => x.Key.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                .Sum(x => RestMinutes(x.Value.Gross));
            weeks.Add(new(week.Key, groups[0], gross, rest, Math.Max(0, groups[0] + gross - rest - 2700)));
        }
        return weeks;
    }
}
