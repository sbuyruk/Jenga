using Jenga.Models.Ortak;

namespace Jenga.DataAccess.Repositories.IRepository.Ortak
{
    public interface IBolgeRepository : IRepository<Bolge>
    {
        Task<Bolge?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    }
}