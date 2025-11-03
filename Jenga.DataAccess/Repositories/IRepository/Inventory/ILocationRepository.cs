using Jenga.Models.Inventory;

namespace Jenga.DataAccess.Repositories.IRepository.Inventory
{
    public interface ILocationRepository : IRepository<Location>
    {
        Task<Location?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    }
}