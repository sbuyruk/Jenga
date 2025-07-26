using Jenga.Models.Ortak;

namespace Jenga.DataAccess.Repositories.IRepository.Ortak
{
    public interface IIlceRepository : IRepository<Ilce>
    {
        Task<List<Ilce>> GetByIlIdAsync(int ilId);
    }
}
