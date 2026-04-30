using Jenga.Models.Inventory;

namespace Jenga.DataAccess.Services.Inventory
{
    public interface IMaterialTransferService
    {
        Task<bool> AddAsync(MaterialTransfer transfer, string? modifiedBy = null, List<int>? selectedAssetIds = null, CancellationToken cancellationToken = default);
    }
}