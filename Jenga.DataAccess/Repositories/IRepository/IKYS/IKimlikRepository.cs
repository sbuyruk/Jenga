using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Repositories.IRepository.IKYS;

public interface IKimlikRepository : IRepository<Kimlik>
{
    Task SaveAsync(CancellationToken cancellationToken = default);
}
