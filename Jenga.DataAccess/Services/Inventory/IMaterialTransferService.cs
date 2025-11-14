using Jenga.Models.Inventory;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.Inventory
{
    public interface IMaterialTransferService
    {
        Task<List<MaterialTransfer>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<MaterialTransfer?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> AddAsync(MaterialTransfer transfer, string? modifiedBy = null, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(MaterialTransfer transfer, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(MaterialTransfer transfer, CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(Expression<Func<MaterialTransfer, bool>> predicate);

        Task<bool> UpdateMaterialTransferAndInventoryAsync(MaterialTransfer transfer, string? currentUserName, CancellationToken cancellationToken = default);
        Task<bool> DeleteMaterialTransferAndUpdateInventoryAsync(MaterialTransfer transfer, string? currentUserName, CancellationToken cancellationToken = default);
    }
}