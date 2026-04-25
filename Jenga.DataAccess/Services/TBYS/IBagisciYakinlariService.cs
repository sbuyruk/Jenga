using Jenga.Models.TBYS;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.TBYS
{
    public interface IBagisciYakinlariService
    {
        Task<List<BagisciYakinlari>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<BagisciYakinlari?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<List<BagisciYakinlari>> GetByBagisciIdAsync(long bagisciId, CancellationToken cancellationToken = default);
        Task<bool> AddAsync(BagisciYakinlari entity, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(BagisciYakinlari entity, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(Expression<Func<BagisciYakinlari, bool>> predicate, CancellationToken cancellationToken = default);
    }
}
