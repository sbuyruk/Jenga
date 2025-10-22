using Jenga.Models.Common;

namespace Jenga.DataAccess.Repositories.IRepository.Menu
{

    public interface IRolRepository : IRepository<Rol>
    {
        Task<Rol?> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default);
    }

}
