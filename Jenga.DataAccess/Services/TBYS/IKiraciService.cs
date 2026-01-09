using Jenga.Models.TBYS;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.TBYS
{
    public interface IKiraciService
    {
        Task<List<Kiraci>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Kiraci?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> AddAsync(Kiraci kiraci, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(Kiraci kiraci, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int kiraciId, CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(Expression<Func<Kiraci, bool>> predicate, CancellationToken cancellationToken = default);
    }
}
