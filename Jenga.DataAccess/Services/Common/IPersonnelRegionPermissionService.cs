using Jenga.Models.Common;
using Jenga.Utility.Results;

namespace Jenga.DataAccess.Services.Common
{
    public interface IPersonnelRegionPermissionService
    {
        Task<Result<List<PersonnelRegionPermission>>> GetByPersonnelIdAsync(int personnelId, CancellationToken cancellationToken = default);
        Task<Result<List<PersonnelRegionPermission>>> GetByRegionIdAsync(int regionId, CancellationToken cancellationToken = default);
        Task<Result> AddAsync(PersonnelRegionPermission entity, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(int personnelId, int regionId, CancellationToken cancellationToken = default);
        Task<Result> ReplaceForPersonnelAsync(int personnelId, IEnumerable<int> regionIds, string currentUser, CancellationToken cancellationToken = default);
    }
}
