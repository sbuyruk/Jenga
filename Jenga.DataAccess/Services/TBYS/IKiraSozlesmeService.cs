using Jenga.Models.TBYS;
using Jenga.Utility.Results;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.TBYS
{
    public interface IKiraSozlesmeService
    {
        Task<Result<List<KiraSozlesme>>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Result<KiraSozlesme>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result> AddAsync(KiraSozlesme sozlesme, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(KiraSozlesme sozlesme, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(int sozlesmeId, CancellationToken cancellationToken = default);
        Task<Result<bool>> AnyAsync(Expression<Func<KiraSozlesme, bool>> predicate, CancellationToken cancellationToken = default);
    }
}
