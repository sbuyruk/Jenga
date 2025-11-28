using Jenga.Models.Common;

namespace Jenga.DataAccess.Repositories.IRepository.Common
{
    public interface IRoleRepository : IRepository<Role>
    {
        Task<Role?> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default);

        // If you want to keep explicit save on repository, expose async save
        Task SaveAsync(CancellationToken cancellationToken = default);
    }
}