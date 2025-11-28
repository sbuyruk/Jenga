using Jenga.Models.Common;

namespace Jenga.DataAccess.Repositories.IRepository.Common
{
    public interface IIlceRepository : IRepository<Ilce>
    {
        Task SaveAsync(CancellationToken cancellationToken = default);
        Task<List<Ilce>> GetByIlIdAsync(int ilId, CancellationToken cancellationToken = default);
    }
}