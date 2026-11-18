namespace Jenga.Models.IKYS;

public sealed class HarcirahPageData
{
    public List<string> Countries { get; init; } = [];
    public string Country { get; init; } = "";
    public List<HarcirahSeriesOption> Series { get; init; } = [];
    public int? SelectedSeriesId { get; init; }
    public List<Harcirah> Rows { get; init; } = [];
    public HarcirahSeriesUndoAvailability Undo { get; init; } = new(false, "Geri alınacak dönemi seçin.");
}

public sealed record HarcirahSeriesOption(int Id, DateTime StartDate, DateTime? EndDate)
{
    public string Label => $"{StartDate:dd.MM.yyyy} – {(EndDate.HasValue ? EndDate.Value.ToString("dd.MM.yyyy") : "Devam ediyor")}";
}

public sealed record HarcirahUpdateRequest(int Id, decimal Amount, string? Description, decimal OriginalAmount, string? OriginalDescription);
public sealed record HarcirahSeriesUndoAvailability(bool CanUndo, string Message, int? PreviousSeriesId = null, string Version = "");
public sealed record HarcirahUndoSeriesRequest(string Country, int SeriesId, string Version);
public sealed record HarcirahUndoSeriesResult(int PreviousSeriesId, int RemovedCount);
public sealed record HarcirahCreateSeriesRequest(string Country, DateTime StartDate, int ExpectedLatestSeriesId);
public sealed record HarcirahCreateSeriesResult(int SeriesId, int Count);
