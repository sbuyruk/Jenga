using Jenga.Models.Common;
using Jenga.Models.Enums;
using Jenga.Utility.Results;

namespace Jenga.DataAccess.Services.Common
{
    public interface IModulePermissionService
    {
        Task<Result<List<ModulePermission>>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Result<ModulePermission>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<List<ModulePermission>>> GetByModuleAsync(ModuleName module, CancellationToken cancellationToken = default);
    }
}
