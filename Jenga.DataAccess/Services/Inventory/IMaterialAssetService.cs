using Jenga.Models.Inventory;

namespace Jenga.DataAccess.Services.Inventory
{
    public interface IMaterialAssetService
    {
        Task<List<MaterialAsset>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<List<MaterialAsset>> GetByMaterialIdAsync(int materialId, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(MaterialAsset asset, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(MaterialAsset asset, CancellationToken cancellationToken = default);
    }
}
