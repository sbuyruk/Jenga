namespace Jenga.Models.IKYS;

public enum LeaveType
{
    Paid = 1,
    Personal = 2,
    Unpaid = 3,
    Marriage = 4,
    Maternity = 5,
    Paternity = 6,
    Bereavement = 7,
    Nursing = 8
}

public enum LeaveDecision { Check, Reject, Approve }
public enum LeaveListFilter { All, General, Personal }

public sealed record LeaveOption(int Id, string Name);

public sealed class LeaveRequestInput
{
    public int Id { get; set; }
    public int? PersonelId { get; set; }
    public LeaveType? Type { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public int? SubstituteId { get; set; }
    public int? ManagerId { get; set; }
    public string? Address { get; set; }
    public string? Description { get; set; }
    public bool Notify { get; set; } = true;
}

public sealed record LeaveRequestForm(
    LeaveRequestInput Input, List<LeaveOption> People, List<LeaveOption> Types, List<LeaveOption> Signers, List<LeaveOption> Substitutes);

public sealed record LeaveRequestRow(
    int Id, string Person, string TypeName, int? Type, DateTime? Start, DateTime? End,
    string? Duration, string? Unit, IzinTalep.OnayDurumuTipi? Status);

public sealed record LeaveRequestList(List<LeaveRequestRow> Items, int Total);

public sealed record LeaveMovementRow(int Id, string TypeName, int? Type, DateTime? Start, DateTime? End, string? Duration, string? Unit);

public sealed record LeaveRequestDetail(
    LeaveRequestRow Row, string? Address, string? Description, string? Substitute, string? Manager,
    List<IzinDonem> Periods, List<LeaveMovementRow> Movements, string? DocumentUrl);

public sealed record LeaveCalculation(string Duration, string Unit, string? Available, string? Remaining,
    string? PendingDuration = null, string? AvailableAfterPending = null,
    string? CombinedDuration = null, string? BalanceWarning = null);

public sealed record LeaveWriteResult(int Id, bool NotificationFailed = false);
