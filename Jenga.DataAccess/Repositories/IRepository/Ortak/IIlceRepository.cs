using Jenga.Models.Ortak;

namespace Jenga.DataAccess.Repositories.IRepository.Ortak
{
    public interface IIlceRepository : IRepository<Ilce>
    {
        Task SaveAsync(CancellationToken cancellationToken = default);
        Task<List<Ilce>> GetByIlIdAsync(int ilId, CancellationToken cancellationToken = default);
    }
}