using Jenga.Models.Ortak;

namespace Jenga.DataAccess.Repositories.IRepository.Menu
{
    public interface IPersonelMenuRepository : IRepository<PersonelMenu>
    {
        // Prefer async save when using IDbContextFactory-created contexts.
        Task SaveAsync(CancellationToken cancellationToken = default);

        Task<IEnumerable<PersonelMenu>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default);
    }
}