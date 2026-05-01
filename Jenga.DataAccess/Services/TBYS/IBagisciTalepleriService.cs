using Jenga.Models.TBYS;
using Jenga.Utility.Results;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.TBYS
{
    public interface IBagisciTalepleriService
    {
        Task<Result<List<BagisciTalepleri>>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Result<BagisciTalepleri>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<List<BagisciTalepleri>>> GetByBagisciIdAsync(long bagisciId, CancellationToken cancellationToken = default);
        Task<Result> AddAsync(BagisciTalepleri entity, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(BagisciTalepleri entity, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<bool>> AnyAsync(Expression<Func<BagisciTalepleri, bool>> predicate, CancellationToken cancellationToken = default);
    }
}
