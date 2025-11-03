using Jenga.Models.Inventory;

namespace Jenga.DataAccess.Repositories.IRepository.Inventory
{
    public interface IMaterialRepository : IRepository<Material>
    {
        Task<Material?> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default);
    }
}