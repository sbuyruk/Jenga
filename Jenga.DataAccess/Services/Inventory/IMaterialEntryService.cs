using Jenga.Models.Inventory;
using Jenga.Utility.Results;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.Inventory
{
    public interface IMaterialEntryService
    {
        Task<Result<List<MaterialEntry>>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Result<bool>> AnyAsync(Expression<Func<MaterialEntry, bool>> predicate);

        Task<Result> AddAsync(MaterialEntry entry, string? modifiedBy = null, CancellationToken cancellationToken = default);
        Task<Result> UpdateMaterialEntryAndInventoryAsync(MaterialEntry entry, string? currentUserName, CancellationToken cancellationToken = default);
        Task<Result> DeleteMaterialEntryAndUpdateInventoryAsync(MaterialEntry entry, string? currentUserName, CancellationToken cancellationToken = default);
    }
}