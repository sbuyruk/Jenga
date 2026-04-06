using Jenga.Models.TBYS;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Repositories.IRepository.TBYS;

public interface IYasalFaizRepository : IRepository<YasalFaiz>
{
    Task<bool> AnyAsync(Expression<Func<YasalFaiz, bool>> predicate, CancellationToken cancellationToken = default);
}