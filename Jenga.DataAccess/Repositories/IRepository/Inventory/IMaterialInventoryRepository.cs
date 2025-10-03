using Jenga.Models.Inventory;

namespace Jenga.DataAccess.Repositories.IRepository.Inventory
{
    public interface IMaterialInventoryRepository : IRepository<MaterialInventory>
    {
        Task<List<MaterialInventory>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<MaterialInventory?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task AddAsync(MaterialInventory inventory, CancellationToken cancellationToken = default);
    }
}