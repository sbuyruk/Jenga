using Jenga.Models.TBYS;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.TBYS
{
    public interface IVasiyetciService
    {
        Task<List<Vasiyetci>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Vasiyetci?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<List<Vasiyetci>> GetByTCKimlikAsync(long tcKimlik, CancellationToken cancellationToken = default);
        Task<bool> AddAsync(Vasiyetci entity, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(Vasiyetci entity, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(Expression<Func<Vasiyetci, bool>> predicate, CancellationToken cancellationToken = default);
    }
}
