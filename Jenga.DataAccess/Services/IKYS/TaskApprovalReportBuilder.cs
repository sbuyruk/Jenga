using System.Globalization;
using System.Text.RegularExpressions;
using Jenga.Models.IKYS;
using Jenga.Utility.Results;

namespace Jenga.DataAccess.Services.IKYS;

public static class TaskApprovalReportBuilder
{
    private static readonly CultureInfo Tr = new("tr-TR");

    public static Result<List<TaskApprovalReportSection>> Build(IReadOnlyList<TaskApprovalListItem> items)
    {
        var rows = new List<TaskApprovalReportRow>();
        foreach (var item in items)
        {
            var currency = (item.Currency ?? "").Trim().ToUpperInvariant();
            if (currency is "TRY" or "₺") currency = "TL";
            if (currency.Length == 0 || !TryParseAmount(item.AllowanceText, item.Currency, out var net) || net < 0)
                return Result.Failure<List<TaskApprovalReportSection>>(Error.Validation($"#{item.TaskApprovalId} görevinin yevmiye tutarı veya para birimi geçersiz. Kaydı kontrol edin."));
            rows.Add(new() { Task = item, Currency = currency, NetAmount = net,
                GrossAmount = decimal.Round(net * 1.00759m, 2, MidpointRounding.AwayFromZero) });
        }
        // Keep distinct signatory sets on separate documents instead of silently choosing one.
        var sections = rows.GroupBy(x => (x.Task.SpecialistSignature, x.Task.PersonnelDirectorSignature, x.Task.FinanceDirectorSignature))
            .Select(group => new TaskApprovalReportSection
            {
                Specialist = group.Key.SpecialistSignature, PersonnelDirector = group.Key.PersonnelDirectorSignature,
                FinanceDirector = group.Key.FinanceDirectorSignature,
                Rows = group.OrderBy(x => x.Task.FullName, StringComparer.Create(Tr, false)).ThenBy(x => x.Task.PersonelId)
                    .ThenBy(x => x.Task.StartDate).ThenBy(x => x.Task.TaskApprovalId).ToList(),
                Totals = Totals(group)
            }).ToList();
        return Result.Success(sections);
    }

    public static List<TaskApprovalReportTotal> Totals(IEnumerable<TaskApprovalReportRow> rows) => rows.GroupBy(x => x.Currency)
        .OrderBy(x => x.Key).Select(x => new TaskApprovalReportTotal(x.Key, x.Sum(r => r.NetAmount), x.Sum(r => r.GrossAmount))).ToList();

    public static bool TryParseAmount(string? value, string? currency, out decimal amount)
    {
        amount = 0;
        var text = (value ?? "").Trim();
        foreach (var suffix in new[] { currency?.Trim(), "TL", "TRY", "₺" }.Where(x => !string.IsNullOrWhiteSpace(x)))
            if (text.EndsWith(suffix!, StringComparison.OrdinalIgnoreCase)) { text = text[..^suffix!.Length].TrimEnd(); break; }
        return Regex.IsMatch(text, @"^[+-]?(?:\d{1,3}(?:\.\d{3})+|\d+)(?:,\d{1,2})?$")
            && decimal.TryParse(text, NumberStyles.Number, Tr, out amount);
    }
}
