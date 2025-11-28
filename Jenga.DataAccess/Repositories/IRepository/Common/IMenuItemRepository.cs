using Jenga.Models.Common;

namespace Jenga.DataAccess.Repositories.IRepository.Common
{
    public interface IMenuItemRepository : IRepository<Models.Common.MenuItem>
    {
        Task<List<Models.Common.MenuItem>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Models.Common.MenuItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        //Task<MenuItem?> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default);
        Task AddAsync(Models.Common.MenuItem item, CancellationToken cancellationToken = default);
    }
}
