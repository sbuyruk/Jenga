using Jenga.Models.Common;

namespace Jenga.DataAccess.Services.Menu
{
    public interface IMenuItemService
    {
        Task<List<MenuItem>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<MenuItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> AddAsync(MenuItem menuItem, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(MenuItem menuItem, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(MenuItem menuItem, CancellationToken cancellationToken = default);
    }
}