using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Repositories.IRepository.IKYS;

public interface IIzinDonemRepository : IRepository<IzinDonem>
{
    Task SaveAsync(CancellationToken cancellationToken = default);
}
