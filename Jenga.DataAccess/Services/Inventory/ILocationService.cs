using Jenga.Models.Inventory;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.Inventory
{
    public interface ILocationService
    {
        Task<List<Location>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Location?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> AddAsync(Location location, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(Location location, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(Location location, CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(Expression<Func<Location, bool>> predicate);
        Task<(bool CanDelete, string? Reason)> CanDeleteAsync(int locationId);
    }
}