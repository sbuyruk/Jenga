using Jenga.Models.Inventory;
using Jenga.Utility.Results;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.Inventory
{
    public interface ILocationService
    {
        Task<Result<List<Location>>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Result<Location>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result> AddAsync(Location location, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(Location location, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(Location location, CancellationToken cancellationToken = default);
        Task<Result<bool>> AnyAsync(Expression<Func<Location, bool>> predicate);
        Task<Result<(bool CanDelete, string? Reason)>> CanDeleteAsync(int locationId);
    }
}