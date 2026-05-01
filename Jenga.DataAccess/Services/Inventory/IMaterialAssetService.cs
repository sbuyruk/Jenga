using Jenga.Models.Inventory;
using Jenga.Utility.Results;

namespace Jenga.DataAccess.Services.Inventory
{
    public interface IMaterialAssetService
    {
        Task<Result<List<MaterialAsset>>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Result<List<MaterialAsset>>> GetByMaterialIdAsync(int materialId, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(MaterialAsset asset, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(MaterialAsset asset, CancellationToken cancellationToken = default);
    }
}
