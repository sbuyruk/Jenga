using Jenga.Models.Common;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.Menu
{
    public interface IRolService
    {
        Task<List<Rol>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Rol?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Rol?> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> AddAsync(Rol rol, string? modifiedBy = null, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(Rol rol, string? modifiedBy = null, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(Rol rol, CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(Expression<Func<Rol, bool>> predicate);
    }
}