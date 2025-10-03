using Jenga.Models.Inventory;

namespace Jenga.DataAccess.Repositories.IRepository.Inventory
{
    public interface IMaterialExitRepository : IRepository<MaterialExit>
    {
        Task<List<MaterialExit>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<MaterialExit?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task AddAsync(MaterialExit exit, CancellationToken cancellationToken = default);
    }
}