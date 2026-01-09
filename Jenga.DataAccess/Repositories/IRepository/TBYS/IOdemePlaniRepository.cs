using Jenga.Models.TBYS;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Repositories.IRepository.TBYS
{
    public interface IOdemePlaniRepository : IRepository<OdemePlani>
    {
        Task<bool> AnyAsync(Expression<Func<OdemePlani, bool>> predicate, CancellationToken cancellationToken = default);
    }
}
