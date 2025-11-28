using Jenga.Models.Common;

namespace Jenga.DataAccess.Repositories.IRepository.Common
{
    public interface IPersonelRoleRepository : IRepository<PersonelRole>
    {
        // Prefer async save when using IDbContextFactory-created contexts.
        Task SaveAsync(CancellationToken cancellationToken = default);

        Task<IEnumerable<PersonelRole>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default);
    }
}