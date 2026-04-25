using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Repositories.IRepository.IKYS;

public interface IIzinTanimRepository : IRepository<IzinTanim>
{
    Task SaveAsync(CancellationToken cancellationToken = default);
}
