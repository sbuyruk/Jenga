using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Jenga.Models.Sistem;

namespace Jenga.Models.IKYS;

public enum OvertimeEntryStatus { Draft, Pending, Approved, Rejected, Correction }
public enum OvertimePeriodStatus { Collecting, AwaitingSignature, Finalized }
public enum OvertimeDeliveryStatus { Requested, Issued, Received }

[Table("OvertimeEntry_Table")]
public sealed class OvertimeEntry : BaseModel
{
    public int PersonelId { get; set; }
    public int UnitId { get; set; }
    [Column(TypeName = "date")] public DateTime WorkDate { get; set; }
    [MaxLength(200)] public string FullName { get; set; } = "";
    public int SicilNo { get; set; }
    [MaxLength(200)] public string UnitName { get; set; } = "";
    [MaxLength(200)] public string Title { get; set; } = "";
    public int NormalWeeklyMinutes { get; set; }
    [Required, MaxLength(2000)] public string Reason { get; set; } = "";
    public OvertimeEntryStatus Status { get; set; }
    [MaxLength(2000)] public string? ReviewReason { get; set; }
    public int? PeriodId { get; set; }
    public List<OvertimeInterval> Intervals { get; set; } = [];
    [Timestamp] public byte[] Version { get; set; } = [];
}

[Table("OvertimeInterval_Table")]
public sealed class OvertimeInterval
{
    public int Id { get; set; }
    public int EntryId { get; set; }
    public int StartMinute { get; set; }
    public int EndMinute { get; set; }
}

[Table("OvertimePeriod_Table")]
public sealed class OvertimePeriod : BaseModel
{
    [Required, MaxLength(200)] public string Name { get; set; } = "";
    [Column(TypeName = "date")] public DateTime StartDate { get; set; }
    [Column(TypeName = "date")] public DateTime EndDate { get; set; }
    [Column(TypeName = "date")] public DateTime DueDate { get; set; }
    public OvertimePeriodStatus Status { get; set; }
    public int SignatureRevision { get; set; }
    public DateTime? FinalizedAt { get; set; }
    [MaxLength(200)] public string? SignedFileName { get; set; }
    [MaxLength(100)] public string? SignedContentType { get; set; }
    public byte[]? SignedDocument { get; set; }
    [Timestamp] public byte[] Version { get; set; } = [];
}

[Table("OvertimeDelivery_Table")]
public sealed class OvertimeDelivery : BaseModel
{
    [NotMapped] public int EntryCount { get; set; }
    [NotMapped] public int ApprovedEntryCount { get; set; }
    public int PeriodId { get; set; }
    public int UnitId { get; set; }
    public int? ManagerId { get; set; }
    [MaxLength(200)] public string UnitName { get; set; } = "";
    public OvertimeDeliveryStatus Status { get; set; }
    public int Revision { get; set; }
    public DateTime? IssuedAt { get; set; }
    public DateTime? ReceivedAt { get; set; }
    [Timestamp] public byte[] Version { get; set; } = [];
}

[Table("OvertimeSchedule_Table")]
public sealed class OvertimeSchedule : BaseModel
{
    public int TitleId { get; set; }
    public int WeeklyHours { get; set; }
}

[Table("OvertimeResult_Table")]
public sealed class OvertimePersonResult : BaseModel
{
    public int PeriodId { get; set; }
    public int PersonelId { get; set; }
    [MaxLength(200)] public string FullName { get; set; } = "";
    public int SicilNo { get; set; }
    public int RawMinutes { get; set; }
    public int RoundedMinutes { get; set; }
    [MaxLength(50)] public string RuleVersion { get; set; } = "2026-11-v1";
    public string WeeksJson { get; set; } = "[]";
}

[Table("OvertimeEvent_Table")]
public sealed class OvertimeEvent : BaseModel
{
    public int? EntryId { get; set; }
    public int? PeriodId { get; set; }
    [MaxLength(80)] public string Action { get; set; } = "";
    public string Details { get; set; } = "";
}

[Table("OvertimeDelegation_Table")]
public sealed class OvertimeDelegation : BaseModel
{
    public int DelegatePersonelId { get; set; }
    public int TargetPersonelId { get; set; }
    public bool IsActive { get; set; }
}

public sealed record OvertimePersonOption(int Id, string FullName, string UserName, int SicilNo);
public sealed record OvertimeDelegationLink(int DelegatePersonelId, int TargetPersonelId);
public sealed class OvertimeDelegationAdminData
{
    public List<OvertimePersonOption> People { get; set; } = [];
    public List<OvertimeDelegationLink> Assignments { get; set; } = [];
}

public sealed class OvertimeEntryInput
{
    public int Id { get; set; }
    public int? PersonelId { get; set; }
    public byte[] Version { get; set; } = [];
    public DateTime WorkDate { get; set; } = DateTime.Today;
    public DateTime? EndWorkDate { get; set; }
    public string Reason { get; set; } = "";
    public bool Submit { get; set; }
    public List<OvertimeTimeInput> Intervals { get; set; } = [new()];
}

public sealed class OvertimeTimeInput
{
    public TimeOnly Start { get; set; } = new(17, 0);
    public TimeOnly End { get; set; } = new(19, 0);
    public bool EndsNextDay { get; set; }
    public int StartMinute => Start.Hour * 60 + Start.Minute;
    public int EndMinute => End.Hour * 60 + End.Minute + (EndsNextDay ? 1440 : 0);
}

public sealed record OvertimeWeek(DateTime Monday, int NormalMinutes, int GrossMinutes, int RestMinutes, int OvertimeMinutes);
public sealed record OvertimeTitleOption(int Id, string Name, int? WeeklyHours);
public sealed class OvertimeWorkspace
{
    public int PersonelId { get; set; }
    public string FullName { get; set; } = "";
    public bool IsHr { get; set; }
    public bool IsManager { get; set; }
    public List<OvertimeEntry> Entries { get; set; } = [];
    public List<OvertimePeriod> Periods { get; set; } = [];
    public List<OvertimeDelivery> Deliveries { get; set; } = [];
    public List<OvertimeTitleOption> Titles { get; set; } = [];
    public List<OvertimePersonOption> DelegatedPeople { get; set; } = [];
}

public sealed class OvertimeReport
{
    public OvertimePeriod Period { get; set; } = new();
    public List<OvertimeEntry> Entries { get; set; } = [];
    public List<OvertimePersonResult> Results { get; set; } = [];
    public List<OvertimeEvent> Events { get; set; } = [];
    public bool CanSeeSummary { get; set; }
    public bool IsPreview { get; set; }
    public string Reference { get; set; } = "";
    public Dictionary<int, OvertimeUnitSignatures> UnitSignatures { get; set; } = [];
    public string? DelegateName { get; set; }
    public string? DelegateDuty { get; set; }
    public string PersonnelDirectorName { get; set; } = "Personel ve İdari İşler Direktörü tanımlanmamış";
    public string SummaryDeputyGeneralManagerName { get; set; } = "Bağlı GMY tanımlanmamış";
}

public sealed class OvertimeUnitSignatures
{
    public string ManagerName { get; set; } = "Birim amiri tanımlanmamış";
    public string ManagerDuty { get; set; } = "Birim Amiri";
    public string DeputyGeneralManagerName { get; set; } = "Bağlı GMY tanımlanmamış";
}

public static class OvertimeDisplay
{
    public static string EventDetails(string details)
    {
        if (!details.StartsWith('{')) return details;
        try
        {
            using var json = JsonDocument.Parse(details);
            if (json.RootElement.TryGetProperty("After", out var after))
            {
                var text = "Kaydedilen bilgiler:\n" + SnapshotText(after);
                if (json.RootElement.TryGetProperty("Before", out var before) && before.ValueKind == JsonValueKind.Object)
                    text = "Önceki bilgiler:\n" + SnapshotText(before) + "\n\n" + text;
                return text;
            }
            if (json.RootElement.TryGetProperty("Reason", out var reason))
                return reason.GetString() ?? "";
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException or KeyNotFoundException or FormatException) { }
        return details;
    }

    private static string SnapshotText(JsonElement snapshot)
    {
        var intervals = snapshot.GetProperty("Intervals").EnumerateArray()
            .Select(x => $"{Duration(x.GetProperty("StartMinute").GetInt32())}–{Duration(x.GetProperty("EndMinute").GetInt32())}");
        return $"Tarih: {snapshot.GetProperty("WorkDate").GetDateTime():dd.MM.yyyy}\nÇalışma aralıkları: {string.Join(" / ", intervals)}\nÇalışma sebebi: {snapshot.GetProperty("Reason").GetString()}\nBirim: {snapshot.GetProperty("UnitName").GetString()}\nUnvan: {snapshot.GetProperty("Title").GetString()}\nHaftalık çalışma: {Duration(snapshot.GetProperty("NormalWeeklyMinutes").GetInt32())}";
    }

    public static string Duration(int minutes) => $"{minutes / 60}:{minutes % 60:00}";
    public static string Label(this OvertimeEntryStatus status) => status switch
    {
        OvertimeEntryStatus.Draft => "Taslak", OvertimeEntryStatus.Pending => "Amir onayında",
        OvertimeEntryStatus.Approved => "Onaylandı", OvertimeEntryStatus.Rejected => "Reddedildi",
        _ => "Düzeltme bekliyor"
    };
    public static string Label(this OvertimePeriodStatus status) => status switch
    {
        OvertimePeriodStatus.Collecting => "Birimlerden toplanıyor",
        OvertimePeriodStatus.AwaitingSignature => "GMY ıslak imzasında", _ => "Kesinleşti"
    };
    public static string Label(this OvertimeDeliveryStatus status) => status switch
    {
        OvertimeDeliveryStatus.Requested => "Çizelge bekleniyor",
        OvertimeDeliveryStatus.Issued => "İmzalı teslim bekleniyor", _ => "İK teslim aldı"
    };
}
