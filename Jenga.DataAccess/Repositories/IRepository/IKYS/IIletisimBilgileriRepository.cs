using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Repositories.IRepository.IKYS;

public interface IIletisimBilgileriRepository : IRepository<IletisimBilgileri>
{
    Task SaveAsync(CancellationToken cancellationToken = default);
}
