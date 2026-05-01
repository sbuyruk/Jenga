using Jenga.Models.TBYS;
using Jenga.Utility.Results;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.TBYS
{
    public interface IVasiyetciService
    {
        Task<Result<List<Vasiyetci>>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Result<Vasiyetci>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<List<Vasiyetci>>> GetByTCKimlikAsync(long tcKimlik, CancellationToken cancellationToken = default);
        Task<Result> AddAsync(Vasiyetci entity, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(Vasiyetci entity, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<bool>> AnyAsync(Expression<Func<Vasiyetci, bool>> predicate, CancellationToken cancellationToken = default);
    }
}
