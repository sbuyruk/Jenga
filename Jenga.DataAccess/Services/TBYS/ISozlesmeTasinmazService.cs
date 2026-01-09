using Jenga.Models.TBYS;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.TBYS
{
    public interface ISozlesmeTasinmazService
    {
        Task<List<SozlesmeTasinmaz>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<SozlesmeTasinmaz?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> AddAsync(SozlesmeTasinmaz entity, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(SozlesmeTasinmaz entity, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(Expression<Func<SozlesmeTasinmaz, bool>> predicate, CancellationToken cancellationToken = default);
    }
}
