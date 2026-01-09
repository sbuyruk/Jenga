using Jenga.Models.TBYS;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Repositories.IRepository.TBYS
{
    public interface ISozlesmeTasinmazRepository : IRepository<SozlesmeTasinmaz>
    {
        Task<bool> AnyAsync(Expression<Func<SozlesmeTasinmaz, bool>> predicate, CancellationToken cancellationToken = default);
    }
}
