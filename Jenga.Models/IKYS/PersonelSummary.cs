using Jenga.Models.Enums;

namespace Jenga.Models.IKYS;

public sealed class PersonelSummary
{
    public int PersonelId { get; set; }
    public string FullName { get; set; } = "";
    public int SicilNo { get; set; }
    public PersonelTipi? PersonnelType { get; set; }
    public string? UnitName { get; set; }
    public string? Title { get; set; }
    public string? JobName { get; set; }
    public DateTime? StartDate { get; set; }
    public CalismaDurumu? EmploymentStatus { get; set; }
    public string? OfficeExtension { get; set; }
    public string? WorkEmail { get; set; }
    public List<PersonelSummaryActivity> TaskApprovals { get; set; } = [];
    public List<PersonelSummaryActivity> OvertimeEntries { get; set; } = [];
}

public sealed record PersonelSummaryActivity(int Id, string Title, DateTime? Date, string? Description, string TargetUrl,
    int? ApprovalValue = null, DateTime? EndDate = null, string? Destination = null);
