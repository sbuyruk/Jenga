using Jenga.Models.Inventory;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.Inventory
{
    public interface IMaterialAssetService
    {
        Task<List<MaterialAsset>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<MaterialAsset?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<MaterialAsset?> GetBySerialNumberAsync(string serialNumber, CancellationToken cancellationToken = default);
        Task<MaterialAsset?> GetByBarcodeAsync(string barcode, CancellationToken cancellationToken = default);
        Task<List<MaterialAsset>> GetByMaterialIdAsync(int materialId, CancellationToken cancellationToken = default);
        Task<List<MaterialAsset>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default);
        Task<bool> AddAsync(MaterialAsset asset, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(MaterialAsset asset, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(MaterialAsset asset, CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(Expression<Func<MaterialAsset, bool>> predicate, CancellationToken cancellationToken = default);
    }
}
