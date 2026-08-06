using Jenga.Models.TBYS;
using Jenga.Utility.Results;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.TBYS
{
    public interface IOdemePlaniService
    {
        Task<Result<List<OdemePlani>>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Result<List<OdemePlani>>> GetAllBySozlesmeIdsAsync(IEnumerable<int> sozlesmeIds, CancellationToken cancellationToken = default);
        Task<Result<List<OdemePlaniDashboardItem>>> GetAllForDashboardBySozlesmeIdsAsync(IEnumerable<int> sozlesmeIds, CancellationToken cancellationToken = default);
        Task<Result<OdemePlani>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result> AddAsync(OdemePlani odemePlani, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(OdemePlani odemePlani, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<bool>> AnyAsync(Expression<Func<OdemePlani, bool>> predicate, CancellationToken cancellationToken = default);
    }
}
