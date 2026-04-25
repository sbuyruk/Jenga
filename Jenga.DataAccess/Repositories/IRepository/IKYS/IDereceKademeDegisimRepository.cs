using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Repositories.IRepository.IKYS;

public interface IDereceKademeDegisimRepository : IRepository<DereceKademeDegisim>
{
    Task SaveAsync(CancellationToken cancellationToken = default);
}
