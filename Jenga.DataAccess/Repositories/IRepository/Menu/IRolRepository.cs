using Jenga.Models.Common;

namespace Jenga.DataAccess.Repositories.IRepository.Menu
{

    public interface IRolRepository : IRepository<Rol>
    {
        Task<List<Rol>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Rol?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Rol?> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default);
        Task AddAsync(Rol rol, CancellationToken cancellationToken = default);
    }

}
