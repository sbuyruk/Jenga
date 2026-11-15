using System.Globalization;
using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Services.IKYS;

public static class TaskApprovalAllowanceCalculator
{
    private static readonly CultureInfo Tr = new("tr-TR");

    public static TaskApprovalCalculationResult Calculate(TaskApprovalCalculationInput input, IReadOnlyList<Harcirah> rates)
    {
        var result = new TaskApprovalCalculationResult();
        if (!input.StartDate.HasValue || !input.EndDate.HasValue ||
            !TimeOnly.TryParseExact(input.StartTime, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startTime) ||
            !TimeOnly.TryParseExact(input.EndTime, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var endTime))
            return Fail(result, "Başlangıç ve bitiş tarihleri ile saatlerini girin.");

        var start = input.StartDate.Value.Date + startTime.ToTimeSpan();
        var end = input.EndDate.Value.Date + endTime.ToTimeSpan();
        result.StartDateTime = start;
        result.EndDateTime = end;
        if (end <= start)
        {
            result.IsNegativeDuration = true;
            return Fail(result, "Bitiş tarihi başlangıç tarihinden sonra olmalıdır.");
        }
        if ((end.Date - start.Date).TotalDays > 40)
            return Fail(result, "Görev tarih aralığı 40 günden fazla olamaz.");

        var minutes = (int)(end - start).TotalMinutes;
        result.DurationDays = minutes / 1440;
        result.DurationHours = minutes % 1440 / 60;
        result.DurationMinutes = minutes % 60;
        result.DurationDayText = $"{result.DurationDays} Gün";
        result.DurationHourMinuteText = $"{result.DurationHours} Saat {result.DurationMinutes} Dakika";

        var relevant = rates.Where(x => x.Ulke == input.Country && x.BaslangicTarihi.Date <= end.Date && EffectiveEndDate(x) >= start.Date).ToList();
        // Split at effective-date boundaries; preserve the actual departure and return times.
        var boundaries = relevant.Select(x => x.BaslangicTarihi.Date)
            .Concat(relevant.Where(x => EffectiveEndDate(x) < DateTime.MaxValue.Date).Select(x => EffectiveEndDate(x).AddDays(1)))
            .Where(x => x > start && x < end).Append(start).Append(end).Distinct().Order().ToArray();
        var segments = new List<(DateTime Start, DateTime End, Harcirah Rate)>();
        for (var i = 0; i < boundaries.Length - 1; i++)
        {
            var from = boundaries[i];
            var to = boundaries[i + 1];
            var rate = relevant.Where(x => x.BaslangicTarihi.Date <= from.Date && EffectiveEndDate(x) >= from.Date)
                .OrderByDescending(x => x.BaslangicTarihi).ThenByDescending(x => x.Id).FirstOrDefault();
            if (rate is null || rate.Miktar < 0 || string.IsNullOrWhiteSpace(rate.ParaBirimi))
                return Fail(result, $"{from:dd.MM.yyyy} tarihi için harcırah bilgisi bulunamadı.");
            if (segments.Count > 0 && segments[^1].Rate.Id == rate.Id)
                segments[^1] = (segments[^1].Start, to, rate);
            else
                segments.Add((from, to, rate));
        }
        if (segments.Select(x => x.Rate.ParaBirimi).Distinct(StringComparer.OrdinalIgnoreCase).Count() != 1)
            return Fail(result, "Görev aralığındaki harcırah serilerinin para birimleri farklı. Tanımları kontrol edin.");

        var first = segments[0].Rate;
        result.HarcirahGroupId = first.KadroGrupId;
        result.HarcirahGroup = first.KadroGrubu ?? "Tanımlanmamış";
        result.ApprovalStatus = first.KadroGrupId is 1 or 2 ? Jenga.Models.Enums.ApprovalStatus.NotRequired : Jenga.Models.Enums.ApprovalStatus.PendingApproval;
        result.Currency = first.ParaBirimi;
        result.HasMultipleRates = segments.Count > 1;
        result.DailyAllowance = result.HasMultipleRates ? 0 : first.Miktar;
        var lines = new List<string>();
        foreach (var segment in segments)
        {
            var days = EarnedDays(segment.End - segment.Start);
            result.EarnedDays += days;
            if (!input.HarcirahHesaplansin) continue;
            var amount = days * segment.Rate.Miktar;
            result.TotalAllowance += amount;
            lines.Add($"{segment.Start:dd.MM.yyyy HH:mm} – {segment.End:dd.MM.yyyy HH:mm}: {segment.Rate.Miktar.ToString("N2", Tr)} {result.Currency} x {days.ToString("0.##", Tr)} gün = {amount.ToString("N2", Tr)} {result.Currency}");
        }
        result.EarnedDayText = $"{result.EarnedDays.ToString("0.##", Tr)} gün";
        result.TotalAllowanceText = result.TotalAllowance.ToString("N2", Tr);
        result.ExplanationText = input.HarcirahHesaplansin
            ? string.Join(Environment.NewLine, lines.Append($"Toplam Harcırah = {result.TotalAllowanceText} {result.Currency}"))
            : "Harcırah Hesaplanmadı";
        result.IsValid = true;
        return result;
    }

    // A null end date denotes an open-ended tariff; a newer start date takes precedence.
    private static DateTime EffectiveEndDate(Harcirah rate) => rate.BitisTarihi?.Date ?? DateTime.MaxValue.Date;

    private static decimal EarnedDays(TimeSpan duration)
    {
        var minutes = (int)duration.TotalMinutes;
        var roundedHours = (minutes % 1440 + 59) / 60;
        return minutes / 1440 + (roundedHours == 0 ? 0m : roundedHours > 12 ? 1m : 0.5m);
    }

    private static TaskApprovalCalculationResult Fail(TaskApprovalCalculationResult result, string message)
    {
        result.ErrorMessage = message;
        result.ExplanationText = message;
        return result;
    }
}
