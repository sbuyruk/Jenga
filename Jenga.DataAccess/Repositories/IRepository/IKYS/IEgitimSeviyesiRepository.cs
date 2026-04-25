using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Repositories.IRepository.IKYS;

public interface IEgitimSeviyesiRepository : IRepository<EgitimSeviyesi>
{
    Task SaveAsync(CancellationToken cancellationToken = default);
}
