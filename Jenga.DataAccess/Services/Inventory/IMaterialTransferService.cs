using Jenga.Models.Inventory;
using Jenga.Utility.Results;

namespace Jenga.DataAccess.Services.Inventory
{
    public interface IMaterialTransferService
    {
        Task<Result> AddAsync(MaterialTransfer transfer, string? modifiedBy = null, List<int>? selectedAssetIds = null, CancellationToken cancellationToken = default);
    }
}