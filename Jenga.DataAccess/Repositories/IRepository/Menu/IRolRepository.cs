using Jenga.Models.Common;

namespace Jenga.DataAccess.Repositories.IRepository.Menu
{
    public interface IRolRepository : IRepository<Rol>
    {
        Task<Rol?> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default);

        // If you want to keep explicit save on repository, expose async save
        Task SaveAsync(CancellationToken cancellationToken = default);
    }
}