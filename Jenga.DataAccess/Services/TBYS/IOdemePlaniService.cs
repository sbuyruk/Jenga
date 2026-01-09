using Jenga.Models.TBYS;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.TBYS
{
    public interface IOdemePlaniService
    {
        Task<List<OdemePlani>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<OdemePlani?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> AddAsync(OdemePlani odemePlani, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(OdemePlani odemePlani, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(Expression<Func<OdemePlani, bool>> predicate, CancellationToken cancellationToken = default);
    }
}
