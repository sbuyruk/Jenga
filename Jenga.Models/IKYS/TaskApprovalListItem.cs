using Jenga.Models.Enums;
using Jenga.Models.Helpers;

namespace Jenga.Models.IKYS;

public sealed class TaskApprovalListItem
{
    public int TaskApprovalId { get; set; }
    public int? PersonelId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string? Destination { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? DurationText { get; set; }
    public string? AllowanceText { get; set; }
    public string? DailyAllowanceText { get; set; }
    public string? Currency { get; set; }
    public string? Transportation { get; set; }
    public string? Transfer { get; set; }
    public string? Accommodation { get; set; }
    public string? Description { get; set; }
    public string? RejectionNote { get; set; }
    public bool IsSelected { get; set; }
    public bool IsPaid { get; set; }
    public bool HasCalculationError { get; set; }
    public int ManagerApprovalValue { get; set; }

    public ApprovalStatus ManagerApprovalStatus => (ApprovalStatus)ManagerApprovalValue;
    public string ManagerApprovalDisplay => EnumHelper.GetEnumDescription(ManagerApprovalStatus);
    public bool IsRejected => ManagerApprovalStatus == ApprovalStatus.Rejected;
}
