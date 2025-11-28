using Jenga.Models.Common;
using System.Collections.Generic;

namespace Jenga.DataAccess.Repositories.IRepository.Common
{
    public interface IRoleMenuRepository : IRepository<RoleMenu>
    {
        // Async save when using IDbContextFactory-created contexts.
        Task SaveAsync(CancellationToken cancellationToken = default);

        // Get RoleMenu entries by RoleId
        Task<IEnumerable<RoleMenu>> GetByRolIdAsync(int rolId, CancellationToken cancellationToken = default);
    }
}
