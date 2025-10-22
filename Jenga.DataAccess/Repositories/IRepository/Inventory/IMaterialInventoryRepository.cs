using Jenga.Models.Inventory;

namespace Jenga.DataAccess.Repositories.IRepository.Inventory
{
    public interface IMaterialInventoryRepository : IRepository<MaterialInventory>
    {
        Task<MaterialInventory?> GetByMaterialLocationAsync(int materialId, int locationId, int materialUnitId, CancellationToken cancellationToken = default);
    }
}