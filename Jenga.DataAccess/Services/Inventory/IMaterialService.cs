using System.Linq.Expressions;
using Jenga.Models.Inventory;

namespace Jenga.DataAccess.Services.Inventory
{
    public interface IMaterialService
    {
        Task<List<Material>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Material?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Material?> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> AddAsync(Material material, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(Material material, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(Material material, CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(Expression<Func<Material, bool>> predicate, CancellationToken cancellationToken = default);
    }
}