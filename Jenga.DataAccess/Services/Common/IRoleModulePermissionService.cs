using Jenga.Models.Common;
using Jenga.Models.Enums;
using Jenga.Utility.Results;

namespace Jenga.DataAccess.Services.Common
{
    public interface IRoleModulePermissionService
    {
        Task<Result<List<RoleModulePermission>>> GetByRoleIdAsync(int roleId, CancellationToken cancellationToken = default);
        Task<Result> AddAsync(RoleModulePermission entity, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(int roleId, int modulePermissionId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Rolün TÜM modül izinlerini siler ve yenileriyle değiştirir.
        /// Kapsam koruması yoktur; delegated admin senaryolarında kapsam dışı izinler silinebilir.
        /// </summary>
        [Obsolete("Kapsam koruması olmadığından ReplaceForRoleInScopeAsync kullanın.")]
        Task<Result> ReplaceForRoleAsync(int roleId, IEnumerable<int> modulePermissionIds, string currentUser, CancellationToken cancellationToken = default);

        /// <summary>
        /// Belirtilen role ait izinleri yalnızca <paramref name="allowedModules"/> kapsamında
        /// değiştirir. Kapsam dışı modüllere ait mevcut izinlere dokunulmaz.
        /// Delegated admin senaryosunda kullanılır: her yönetici yalnızca kendi modülünü değiştirebilir.
        /// </summary>
        Task<Result> ReplaceForRoleInScopeAsync(
            int roleId,
            IEnumerable<int> modulePermissionIds,
            IEnumerable<ModuleName> allowedModules,
            string currentUser,
            CancellationToken cancellationToken = default);
    }
}
