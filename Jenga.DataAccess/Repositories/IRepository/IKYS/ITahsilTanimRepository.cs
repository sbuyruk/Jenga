using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Repositories.IRepository.IKYS;

public interface ITahsilTanimRepository : IRepository<TahsilTanim>
{
    Task SaveAsync(CancellationToken cancellationToken = default);
}
