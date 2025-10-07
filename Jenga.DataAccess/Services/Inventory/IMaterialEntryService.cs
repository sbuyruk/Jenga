using Jenga.Models.Inventory;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.Inventory
{
    public interface IMaterialEntryService
    {
        Task<List<MaterialEntry>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<MaterialEntry?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> AddAsync(MaterialEntry entry, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(MaterialEntry entry, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(MaterialEntry entry, CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(Expression<Func<MaterialEntry, bool>> predicate);
    }
}