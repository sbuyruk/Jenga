using Jenga.Models.TBYS;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.TBYS
{
    public interface IKiraSozlesmeService
    {
        Task<List<KiraSozlesme>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<KiraSozlesme?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> AddAsync(KiraSozlesme sozlesme, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(KiraSozlesme sozlesme, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int sozlesmeId, CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(Expression<Func<KiraSozlesme, bool>> predicate, CancellationToken cancellationToken = default);
    }
}
