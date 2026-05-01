using Jenga.Models.TBYS;
using Jenga.Utility.Results;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.TBYS
{
    public interface IKiraciService
    {
        Task<Result<List<Kiraci>>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Result<Kiraci>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result> AddAsync(Kiraci kiraci, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(Kiraci kiraci, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(int kiraciId, CancellationToken cancellationToken = default);
        Task<Result<bool>> AnyAsync(Expression<Func<Kiraci, bool>> predicate, CancellationToken cancellationToken = default);
    }
}
