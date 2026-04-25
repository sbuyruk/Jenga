using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Repositories.IRepository.IKYS;

public interface IGorevTanimRepository : IRepository<GorevTanim>
{
    Task SaveAsync(CancellationToken cancellationToken = default);
}
