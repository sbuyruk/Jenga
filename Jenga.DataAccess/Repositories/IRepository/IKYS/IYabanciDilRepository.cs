using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Repositories.IRepository.IKYS;

public interface IYabanciDilRepository : IRepository<YabanciDil>
{
    Task SaveAsync(CancellationToken cancellationToken = default);
}
