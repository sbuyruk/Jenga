namespace Jenga.Models.IKYS;

public sealed class PendingApprovalsOverview
{
    public bool HasManagerUnits { get; init; }
    public List<PendingApprovalItem> ReviewItems { get; init; } = [];
    public List<PendingApprovalItem> PendingTasks { get; init; } = [];
    public List<PendingApprovalItem> RecentApprovedTasks { get; init; } = [];
}
