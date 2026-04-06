using Jenga.Models.Inventory;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.Inventory
{
    public interface IMaterialAssetLogService
    {
        Task<List<MaterialAssetLog>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<MaterialAssetLog?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<List<MaterialAssetLog>> GetByAssetIdAsync(int materialAssetId, CancellationToken cancellationToken = default);
        Task<bool> AddAsync(MaterialAssetLog log, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(MaterialAssetLog log, CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(Expression<Func<MaterialAssetLog, bool>> predicate, CancellationToken cancellationToken = default);
    }
}
