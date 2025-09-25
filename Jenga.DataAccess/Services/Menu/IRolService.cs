using Jenga.Models.Common;

namespace Jenga.DataAccess.Services.Menu
{
    public interface IRolService
    {
        Task<List<Rol>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Rol?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> AddAsync(Rol rol, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(Rol rol, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<Rol?> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default);
    }
}
