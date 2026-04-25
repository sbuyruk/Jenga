using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Repositories.IRepository.IKYS;

public interface IIzinHareketRepository : IRepository<IzinHareket>
{
    Task SaveAsync(CancellationToken cancellationToken = default);
}
