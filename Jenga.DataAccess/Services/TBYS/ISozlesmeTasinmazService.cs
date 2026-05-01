using Jenga.Models.TBYS;
using Jenga.Utility.Results;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.TBYS
{
    public interface ISozlesmeTasinmazService
    {
        Task<Result<List<SozlesmeTasinmaz>>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Result<SozlesmeTasinmaz>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result> AddAsync(SozlesmeTasinmaz entity, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(SozlesmeTasinmaz entity, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<bool>> AnyAsync(Expression<Func<SozlesmeTasinmaz, bool>> predicate, CancellationToken cancellationToken = default);
    }
}
