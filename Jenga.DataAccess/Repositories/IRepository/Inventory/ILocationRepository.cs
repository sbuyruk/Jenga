using Jenga.Models.Inventory;

namespace Jenga.DataAccess.Repositories.IRepository.Inventory
{
    public interface ILocationRepository : IRepository<Location>
    {
        Task<List<Location>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Location?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task AddAsync(Location location, CancellationToken cancellationToken = default);
    }
}