using Jenga.Models.TBYS;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Repositories.IRepository.TBYS
{
    public interface IKiraSozlesmeRepository : IRepository<KiraSozlesme>
    {
        Task<bool> AnyAsync(Expression<Func<KiraSozlesme, bool>> predicate, CancellationToken cancellationToken = default);
    }
}
