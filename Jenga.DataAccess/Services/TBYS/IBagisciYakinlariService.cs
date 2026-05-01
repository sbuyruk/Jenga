using Jenga.Models.TBYS;
using Jenga.Utility.Results;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.TBYS
{
    public interface IBagisciYakinlariService
    {
        Task<Result<List<BagisciYakinlari>>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Result<BagisciYakinlari>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<List<BagisciYakinlari>>> GetByBagisciIdAsync(long bagisciId, CancellationToken cancellationToken = default);
        Task<Result> AddAsync(BagisciYakinlari entity, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(BagisciYakinlari entity, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<bool>> AnyAsync(Expression<Func<BagisciYakinlari, bool>> predicate, CancellationToken cancellationToken = default);
    }
}
