using Jenga.Models.Common;

namespace Jenga.DataAccess.Repositories.IRepository.Menu
{
    public interface IMenuItemRepository : IRepository<MenuItem>
    {
        Task<List<MenuItem>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<MenuItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        //Task<MenuItem?> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default);
        Task AddAsync(MenuItem item, CancellationToken cancellationToken = default);
    }
}
