using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Repositories.IRepository.IKYS;

public interface IAileRepository : IRepository<Aile>
{
    Task SaveAsync(CancellationToken cancellationToken = default);
}
