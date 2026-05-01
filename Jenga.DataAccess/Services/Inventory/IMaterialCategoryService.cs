using Jenga.Models.Inventory;
using Jenga.Utility.Results;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.Inventory
{
    public interface IMaterialCategoryService
    {
        Task<Result<List<MaterialCategory>>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Result<MaterialCategory>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result> AddAsync(MaterialCategory category, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(MaterialCategory category, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(int categoryId, CancellationToken cancellationToken = default);
        Task<Result<bool>> AnyAsync(Expression<Func<MaterialCategory, bool>> predicate, CancellationToken cancellationToken = default);
        Task<Result<(bool CanDelete, string? Reason)>> CanDeleteAsync(int id);
    }
}