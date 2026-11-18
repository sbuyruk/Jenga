using Jenga.Models.IKYS;
using Jenga.Utility.Results;

namespace Jenga.DataAccess.Services.IKYS;

public interface IHarcirahService
{
    Task<Result<HarcirahPageData>> GetAsync(string? country = null, int? seriesId = null, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(HarcirahUpdateRequest request, CancellationToken cancellationToken = default);
    Task<Result<HarcirahUndoSeriesResult>> UndoSeriesAsync(HarcirahUndoSeriesRequest request, CancellationToken cancellationToken = default);
    Task<Result<HarcirahCreateSeriesResult>> CreateSeriesAsync(HarcirahCreateSeriesRequest request, CancellationToken cancellationToken = default);
}
