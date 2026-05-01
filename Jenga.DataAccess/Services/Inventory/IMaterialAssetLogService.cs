using Jenga.Models.Inventory;
using Jenga.Utility.Results;

namespace Jenga.DataAccess.Services.Inventory
{
    public interface IMaterialAssetLogService
    {
        Task<Result<List<MaterialAssetLog>>> GetByAssetIdAsync(int materialAssetId, CancellationToken cancellationToken = default);
    }
}
