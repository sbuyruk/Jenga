using Jenga.Models.Common;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Jenga.DataAccess.Services.Common
{
    public interface IRoleService
    {
        Task<bool> AddAsync(Role role, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(Role role, CancellationToken cancellationToken = default);

        // New: transactional methods that handle related join tables in one DbContext/repository flow
        Task<bool> AddWithRelationsAsync(Role role, CancellationToken cancellationToken = default);
        Task<bool> UpdateWithRelationsAsync(Role role, CancellationToken cancellationToken = default);

        Task<Role?> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(Role role, CancellationToken cancellationToken = default);

        // Added: get all roles
        Task<List<Role>> GetAllAsync(CancellationToken cancellationToken = default);
    }
}
