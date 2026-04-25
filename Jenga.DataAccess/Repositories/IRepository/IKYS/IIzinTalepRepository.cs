using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Repositories.IRepository.IKYS;

public interface IIzinTalepRepository : IRepository<IzinTalep>
{
    Task SaveAsync(CancellationToken cancellationToken = default);
}
