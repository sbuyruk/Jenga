using Jenga.Models.Inventory;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Repositories.IRepository.Inventory
{
    public interface IMaterialEntryRepository : IRepository<MaterialEntry>
    {
        Task<bool> AnyAsync(Expression<Func<MaterialEntry, bool>> predicate);
    }
}