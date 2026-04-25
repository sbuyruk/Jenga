using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Repositories.IRepository.IKYS;

public interface IIsBilgileriRepository : IRepository<IsBilgileri>
{
    Task SaveAsync(CancellationToken cancellationToken = default);
}
