using Jenga.Models.Inventory;

namespace Jenga.DataAccess.Repositories.IRepository.Inventory
{
    public interface IMaterialRepository : IRepository<Material>
    {
        Task<List<Material>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Material?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Material?> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default);
        Task AddAsync(Material material, CancellationToken cancellationToken = default);
    }
}