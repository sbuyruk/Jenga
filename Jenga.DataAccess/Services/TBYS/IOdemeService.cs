using Jenga.Models.TBYS;
using Jenga.Utility.Results;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.TBYS
{
    public interface IOdemeService
    {
        Task<Result<List<Odeme>>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Result<List<Odeme>>> GetAllAsyncKiralar(CancellationToken cancellationToken = default);
        Task<Result<List<OdemeDashboardItem>>> GetLastYearsForDashboardKiralarAsync(int years, CancellationToken cancellationToken = default);
        Task<Result<List<OdemeBolgeDashboardItem>>> GetAllForBolgeDashboardBySozlesmeIdsAsync(IEnumerable<int> sozlesmeIds, CancellationToken cancellationToken = default);
        Task<Result<Odeme>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<Odeme>> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<List<Odeme>>> GetBySozlesmeIdAsync(int sozlesmeId, CancellationToken cancellationToken = default);
        Task<Result<List<Odeme>>> GetByKiraciIdAsync(int kiraciId, CancellationToken cancellationToken = default);
        Task<Result<List<Odeme>>> GetByOdemePlaniIdAsync(int odemePlaniId, CancellationToken cancellationToken = default);

        Task<Result> AddAsync(Odeme odeme, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(Odeme odeme, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(int odemeId, CancellationToken cancellationToken = default);

        Task<Result<bool>> AnyAsync(Expression<Func<Odeme, bool>> predicate, CancellationToken cancellationToken = default);
        Task<Result<(bool CanDelete, string? Reason)>> CanDeleteAsync(int id);
    }
}
