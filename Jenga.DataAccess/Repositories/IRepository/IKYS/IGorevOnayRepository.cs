using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Repositories.IRepository.IKYS;

public interface IGorevOnayRepository : IRepository<GorevOnay>
{
    Task SaveAsync(CancellationToken cancellationToken = default);
}
