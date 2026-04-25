using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Repositories.IRepository.IKYS;

public interface IBirimTanimRepository : IRepository<BirimTanim>
{
    Task SaveAsync(CancellationToken cancellationToken = default);
}
