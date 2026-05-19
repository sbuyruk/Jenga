using Jenga.Models.Common;
using Jenga.Utility.Results;

namespace Jenga.DataAccess.Services.Common
{
    public interface IRoleModulePermissionService
    {
        Task<Result<List<RoleModulePermission>>> GetByRoleIdAsync(int roleId, CancellationToken cancellationToken = default);
        Task<Result> AddAsync(RoleModulePermission entity, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(int roleId, int modulePermissionId, CancellationToken cancellationToken = default);
        Task<Result> ReplaceForRoleAsync(int roleId, IEnumerable<int> modulePermissionIds, string currentUser, CancellationToken cancellationToken = default);
    }
}
