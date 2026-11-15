namespace Jenga.Models.IKYS;

public sealed record TaskApprovalReportSignature(int? PersonelId = null, string Name = "", string Title = "", bool IsDeputy = false);

public sealed class TaskApprovalReportRow
{
    public required TaskApprovalListItem Task { get; init; }
    public required string Currency { get; init; }
    public decimal NetAmount { get; init; }
    public decimal GrossAmount { get; init; }
}

public sealed record TaskApprovalReportTotal(string Currency, decimal NetAmount, decimal GrossAmount);

public sealed class TaskApprovalReportSection
{
    public required TaskApprovalReportSignature Specialist { get; init; }
    public required TaskApprovalReportSignature PersonnelDirector { get; init; }
    public required TaskApprovalReportSignature FinanceDirector { get; init; }
    public required IReadOnlyList<TaskApprovalReportRow> Rows { get; init; }
    public required IReadOnlyList<TaskApprovalReportTotal> Totals { get; init; }
}
