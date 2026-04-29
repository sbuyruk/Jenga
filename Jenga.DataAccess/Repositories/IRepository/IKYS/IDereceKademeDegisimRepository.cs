using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Repositories.IRepository.IKYS;

public interface IDereceKademeDegisimRepository : IRepository<DereceKademeDegisim>
{
    Task<List<DereceKademeDegisim>> GetDereceYukseltmeAsync(CancellationToken cancellationToken);
    Task SaveAsync(CancellationToken cancellationToken = default);
}
