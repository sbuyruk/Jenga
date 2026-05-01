using Jenga.Models.Common;
using Jenga.Utility.Results;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Jenga.DataAccess.Services.Common
{
    public interface IRoleService
    {
        Task<Result> AddAsync(Role role, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(Role role, CancellationToken cancellationToken = default);

        // New: transactional methods that handle related join tables in one DbContext/repository flow
        Task<Result> AddWithRelationsAsync(Role role, CancellationToken cancellationToken = default);
        Task<Result> UpdateWithRelationsAsync(Role role, CancellationToken cancellationToken = default);

        Task<Result<Role>> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(Role role, CancellationToken cancellationToken = default);

        // Added: get all roles
        Task<Result<List<Role>>> GetAllAsync(CancellationToken cancellationToken = default);
    }
}
