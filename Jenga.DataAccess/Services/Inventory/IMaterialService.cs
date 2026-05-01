using Jenga.Models.Inventory;
using Jenga.Utility.Results;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.Inventory
{
    public interface IMaterialService
    {
        Task<Result<List<Material>>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Result<Material>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<Material>> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default);
        Task<Result> AddAsync(Material material, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(Material material, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(int materialId, CancellationToken cancellationToken = default);
        Task<Result<bool>> AnyAsync(Expression<Func<Material, bool>> predicate, CancellationToken cancellationToken = default);
        Task<Result<(bool CanDelete, string? Reason)>> CanDeleteAsync(int id);
        Task<Result<bool>> ExistsByNameAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default);
    }
}