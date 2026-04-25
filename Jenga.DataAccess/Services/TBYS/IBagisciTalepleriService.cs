using Jenga.Models.TBYS;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.TBYS
{
    public interface IBagisciTalepleriService
    {
        Task<List<BagisciTalepleri>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<BagisciTalepleri?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<List<BagisciTalepleri>> GetByBagisciIdAsync(long bagisciId, CancellationToken cancellationToken = default);
        Task<bool> AddAsync(BagisciTalepleri entity, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(BagisciTalepleri entity, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(Expression<Func<BagisciTalepleri, bool>> predicate, CancellationToken cancellationToken = default);
    }
}
