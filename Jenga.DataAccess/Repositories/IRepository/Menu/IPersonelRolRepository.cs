using Jenga.Models.Common;

namespace Jenga.DataAccess.Repositories.IRepository.Menu
{
    public interface IPersonelRolRepository : IRepository<PersonelRol>
    {
        // Prefer async save when using IDbContextFactory-created contexts.
        Task SaveAsync(CancellationToken cancellationToken = default);

        Task<IEnumerable<PersonelRol>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default);
    }
}