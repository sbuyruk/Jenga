using Jenga.Models.Inventory;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.Inventory
{
    public interface IMaterialService
    {
        Task<List<Material>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Material?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Material?> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> AddAsync(Material material, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(Material material, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int materialId, CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(Expression<Func<Material, bool>> predicate, CancellationToken cancellationToken = default);
        Task<(bool CanDelete, string? Reason)> CanDeleteAsync(int id);
        Task<bool> ExistsByNameAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default);
    }
}