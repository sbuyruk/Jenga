using Jenga.Models.IKYS;
using Jenga.Utility.Results;

namespace Jenga.DataAccess.Services.IKYS;

public interface ITaskApprovalEntryService
{
    Task<Result<TaskApprovalEntryLoadResult>> LoadAsync(TaskApprovalEntryLoadRequest request, CancellationToken cancellationToken = default);
    Task<TaskApprovalCalculationResult> CalculateAsync(TaskApprovalCalculationInput input, CancellationToken cancellationToken = default);
    Task<Result<bool>> HasOverlapAsync(int personelId, DateTime startDate, DateTime endDate, int? excludeTaskApprovalId = null, CancellationToken cancellationToken = default);
    Task<Result<TaskApprovalEntrySaveResult>> SaveAsync(TaskApprovalEntrySaveRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int id, int? currentPersonelId, string? auth, CancellationToken cancellationToken = default);
}
