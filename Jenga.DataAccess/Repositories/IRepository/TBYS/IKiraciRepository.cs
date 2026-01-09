using Jenga.Models.TBYS;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Repositories.IRepository.TBYS
{
    public interface IKiraciRepository : IRepository<Kiraci>
    {
        Task<bool> AnyAsync(Expression<Func<Kiraci, bool>> predicate, CancellationToken cancellationToken = default);
    }
}
