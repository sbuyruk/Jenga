using Jenga.Models.Inventory;
using Jenga.Utility.Results;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.Inventory
{
    public interface IMaterialExitService
    {
        Task<Result<List<MaterialExit>>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Result> AddAsync(MaterialExit exit, List<int>? selectedAssetIds = null, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(MaterialExit exit, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(MaterialExit exit, CancellationToken cancellationToken = default);
        Task<Result<bool>> AnyAsync(Expression<Func<MaterialExit, bool>> predicate);
    }
}