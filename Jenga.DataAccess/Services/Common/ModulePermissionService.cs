using Jenga.DataAccess.Data;
using Jenga.Models.Common;
using Jenga.Models.Enums;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.Common
{
    public class ModulePermissionService : IModulePermissionService
    {
        private const string Source = nameof(ModulePermissionService);
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService _logService;

        public ModulePermissionService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        }

        public async Task<Result<List<ModulePermission>>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.ModulePermission_Table.AsNoTracking().ToListAsync(cancellationToken);
                return Result.Success(list);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.GetAllAsync");
                return Result.Failure<List<ModulePermission>>(Error.Unexpected("Modül izinleri getirilemedi.", ex, "ModulePermission.GetAll.Failed"));
            }
        }

        public async Task<Result<ModulePermission>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.ModulePermission_Table.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                if (entity is null)
                    return Result.Failure<ModulePermission>(Error.NotFound($"Modül izni bulunamadı (Id={id}).", "ModulePermission.NotFound"));
                return Result.Success(entity);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.GetByIdAsync");
                return Result.Failure<ModulePermission>(Error.Unexpected("Modül izni getirilemedi.", ex, "ModulePermission.GetById.Failed"));
            }
        }

        public async Task<Result<List<ModulePermission>>> GetByModuleAsync(ModuleName module, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.ModulePermission_Table.AsNoTracking()
                    .Where(x => x.Module == module)
                    .ToListAsync(cancellationToken);
                return Result.Success(list);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.GetByModuleAsync");
                return Result.Failure<List<ModulePermission>>(Error.Unexpected("Modül izinleri getirilemedi.", ex, "ModulePermission.GetByModule.Failed"));
            }
        }
    }
}
