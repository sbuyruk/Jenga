using Jenga.Models.Inventory;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.Inventory
{
    public interface IMaterialEntryService
    {
        Task<List<MaterialEntry>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<MaterialEntry?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(MaterialEntry entry, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(MaterialEntry entry, CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(Expression<Func<MaterialEntry, bool>> predicate);

        Task<bool> AddAsync(MaterialEntry entry, string? modifiedBy = null, CancellationToken cancellationToken = default);
        Task<bool> UpdateMaterialEntryAndInventoryAsync(MaterialEntry entry, string? currentUserName, CancellationToken cancellationToken = default);
        Task<bool> DeleteMaterialEntryAndUpdateInventoryAsync(MaterialEntry entry, string? currentUserName, CancellationToken cancellationToken = default);
    }
}