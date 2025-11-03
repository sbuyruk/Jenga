using Jenga.Models.Inventory;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Repositories.IRepository.Inventory
{
    public interface IMaterialCategoryRepository : IRepository<MaterialCategory>
    {
        Task<bool> AnyAsync(Expression<Func<MaterialCategory, bool>> predicate, CancellationToken cancellationToken = default);
    }
}