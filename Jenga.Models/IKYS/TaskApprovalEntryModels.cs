using Jenga.Models.Enums;

namespace Jenga.Models.IKYS;

public sealed class TaskApprovalEntryLoadRequest
{
    public int? GorevOnayId { get; set; }
    public int? CurrentPersonelId { get; set; }
    public int? QueryPersonelId { get; set; }
    public int? QueryBirimTanimId { get; set; }
    public string? Auth { get; set; }
    public bool CanManage { get; set; }
}

public sealed class TaskApprovalEntryLoadResult
{
    public GorevOnay Model { get; set; } = new();
    public bool IsEditMode { get; set; }
    public bool ShowSave { get; set; }
    public bool ShowUpdate { get; set; }
    public bool ShowDelete { get; set; }
    public bool ShowReport { get; set; }
    public bool IsPaid { get; set; }
    public string? SelectedPersonDisplayName { get; set; }
    public string HarcirahGroup { get; set; } = "Standart";
    public string SelectedCountry { get; set; } = "Türkiye";
    public bool HarcirahHesaplansin { get; set; } = true;
    public List<TaskApprovalOptionItem> PersonOptions { get; set; } = [];
    public List<TaskApprovalOptionItem> PerDirOptions { get; set; } = [];
    public List<TaskApprovalOptionItem> PerUzmOptions { get; set; } = [];
    public List<string> UlasimAraclari { get; set; } = [];
    public List<string> TransferSecenekleri { get; set; } = [];
    public List<string> KonaklamaSecenekleri { get; set; } = [];
    public List<TaskApprovalCountryRateOption> CountryRates { get; set; } = [];
}

public sealed class TaskApprovalOptionItem
{
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty;
}

public sealed class TaskApprovalCountryRateOption
{
    public string Country { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal DailyAmount { get; set; }
}

public sealed class TaskApprovalCalculationInput
{
    public DateTime? StartDate { get; set; }
    public string? StartTime { get; set; }
    public DateTime? EndDate { get; set; }
    public string? EndTime { get; set; }
    public string Country { get; set; } = "Türkiye";
    public bool HarcirahHesaplansin { get; set; } = true;
}

public sealed class TaskApprovalCalculationResult
{
    public DateTime? StartDateTime { get; set; }
    public DateTime? EndDateTime { get; set; }
    public bool IsNegativeDuration { get; set; }
    public int DurationDays { get; set; }
    public int DurationHours { get; set; }
    public int DurationMinutes { get; set; }
    public decimal EarnedDays { get; set; }
    public decimal DailyAllowance { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal TotalAllowance { get; set; }
    public string DurationDayText { get; set; } = "0 Gün";
    public string DurationHourMinuteText { get; set; } = "0 Saat 0 Dakika";
    public string EarnedDayText { get; set; } = "0 gün";
    public string TotalAllowanceText { get; set; } = "0";
    public string ExplanationText { get; set; } = string.Empty;
}
