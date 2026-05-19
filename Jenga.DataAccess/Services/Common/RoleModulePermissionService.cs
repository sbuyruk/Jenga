using Jenga.DataAccess.Data;
using Jenga.Models.Common;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.Common
{
    public class RoleModulePermissionService : IRoleModulePermissionService
    {
        private const string Source = nameof(RoleModulePermissionService);
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly IDbContextScopeFactory _scopeFactory;
        private readonly ILogService _logService;

        public RoleModulePermissionService(
            IDbContextFactory<ApplicationDbContext> dbFactory,
            IDbContextScopeFactory scopeFactory,
            ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        }

        public async Task<Result<List<RoleModulePermission>>> GetByRoleIdAsync(int roleId, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.RoleModulePermission_Table
                    .AsNoTracking()
                    .Include(x => x.ModulePermission)
                    .Where(x => x.RoleId == roleId)
                    .ToListAsync(cancellationToken);
                return Result.Success(list);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.GetByRoleIdAsync");
                return Result.Failure<List<RoleModulePermission>>(Error.Unexpected("Rol izinleri getirilemedi.", ex, "RoleModulePermission.GetByRole.Failed"));
            }
        }

        public async Task<Result> AddAsync(RoleModulePermission entity, CancellationToken cancellationToken = default)
        {
            if (entity is null)
                return Result.Failure(Error.Validation("Rol izni boş olamaz.", "RoleModulePermission.Null"));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                await db.RoleModulePermission_Table.AddAsync(entity, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.AddAsync");
                return Result.Failure(Error.Unexpected("Rol izni eklenemedi.", ex, "RoleModulePermission.Add.Failed"));
            }
        }

        public async Task<Result> DeleteAsync(int roleId, int modulePermissionId, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.RoleModulePermission_Table
                    .FirstOrDefaultAsync(x => x.RoleId == roleId && x.ModulePermissionId == modulePermissionId, cancellationToken);
                if (entity is null)
                    return Result.Failure(Error.NotFound("Rol izni bulunamadı.", "RoleModulePermission.NotFound"));
                db.RoleModulePermission_Table.Remove(entity);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.DeleteAsync");
                return Result.Failure(Error.Unexpected("Rol izni silinemedi.", ex, "RoleModulePermission.Delete.Failed"));
            }
        }

        /// <summary>
        /// Belirtilen role ait tüm modül izinlerini siler, yenileriyle değiştirir.
        /// Tek transaction içinde çalışır.
        /// </summary>
        public async Task<Result> ReplaceForRoleAsync(int roleId, IEnumerable<int> modulePermissionIds, string currentUser, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var scope = await _scopeFactory.CreateAsync(cancellationToken);
                var db = scope.Context;
                db.SetCurrentUser(currentUser);

                var existing = await db.RoleModulePermission_Table
                    .Where(x => x.RoleId == roleId)
                    .ToListAsync(cancellationToken);
                db.RoleModulePermission_Table.RemoveRange(existing);

                var newEntities = modulePermissionIds.Select(mpId => new RoleModulePermission
                {
                    RoleId = roleId,
                    ModulePermissionId = mpId
                });
                await db.RoleModulePermission_Table.AddRangeAsync(newEntities, cancellationToken);

                await scope.CommitAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.ReplaceForRoleAsync");
                return Result.Failure(Error.Unexpected("Rol izinleri güncellenemedi.", ex, "RoleModulePermission.Replace.Failed"));
            }
        }
    }
}
