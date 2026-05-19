using Jenga.DataAccess.Data;
using Jenga.Models.Common;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.Common
{
    public class PersonnelRegionPermissionService : IPersonnelRegionPermissionService
    {
        private const string Source = nameof(PersonnelRegionPermissionService);
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly IDbContextScopeFactory _scopeFactory;
        private readonly ILogService _logService;

        public PersonnelRegionPermissionService(
            IDbContextFactory<ApplicationDbContext> dbFactory,
            IDbContextScopeFactory scopeFactory,
            ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        }

        public async Task<Result<List<PersonnelRegionPermission>>> GetByPersonnelIdAsync(int personnelId, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.PersonnelRegionPermission_Table
                    .AsNoTracking()
                    .Include(x => x.Region)
                    .Where(x => x.PersonnelId == personnelId)
                    .ToListAsync(cancellationToken);
                return Result.Success(list);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.GetByPersonnelIdAsync");
                return Result.Failure<List<PersonnelRegionPermission>>(Error.Unexpected("Personel bölge yetkileri getirilemedi.", ex, "PersonnelRegionPermission.GetByPersonnel.Failed"));
            }
        }

        public async Task<Result<List<PersonnelRegionPermission>>> GetByRegionIdAsync(int regionId, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.PersonnelRegionPermission_Table
                    .AsNoTracking()
                    .Include(x => x.Personnel)
                    .Where(x => x.RegionId == regionId)
                    .ToListAsync(cancellationToken);
                return Result.Success(list);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.GetByRegionIdAsync");
                return Result.Failure<List<PersonnelRegionPermission>>(Error.Unexpected("Bölge personel yetkileri getirilemedi.", ex, "PersonnelRegionPermission.GetByRegion.Failed"));
            }
        }

        public async Task<Result> AddAsync(PersonnelRegionPermission entity, CancellationToken cancellationToken = default)
        {
            if (entity is null)
                return Result.Failure(Error.Validation("Bölge yetkisi boş olamaz.", "PersonnelRegionPermission.Null"));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                await db.PersonnelRegionPermission_Table.AddAsync(entity, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.AddAsync");
                return Result.Failure(Error.Unexpected("Bölge yetkisi eklenemedi.", ex, "PersonnelRegionPermission.Add.Failed"));
            }
        }

        public async Task<Result> DeleteAsync(int personnelId, int regionId, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.PersonnelRegionPermission_Table
                    .FirstOrDefaultAsync(x => x.PersonnelId == personnelId && x.RegionId == regionId, cancellationToken);
                if (entity is null)
                    return Result.Failure(Error.NotFound("Bölge yetkisi bulunamadı.", "PersonnelRegionPermission.NotFound"));
                db.PersonnelRegionPermission_Table.Remove(entity);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.DeleteAsync");
                return Result.Failure(Error.Unexpected("Bölge yetkisi silinemedi.", ex, "PersonnelRegionPermission.Delete.Failed"));
            }
        }

        /// <summary>
        /// Personelin tüm bölge yetkilerini siler, yenileriyle değiştirir.
        /// Tek transaction içinde çalışır.
        /// </summary>
        public async Task<Result> ReplaceForPersonnelAsync(int personnelId, IEnumerable<int> regionIds, string currentUser, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var scope = await _scopeFactory.CreateAsync(cancellationToken);
                var db = scope.Context;
                db.SetCurrentUser(currentUser);

                var existing = await db.PersonnelRegionPermission_Table
                    .Where(x => x.PersonnelId == personnelId)
                    .ToListAsync(cancellationToken);
                db.PersonnelRegionPermission_Table.RemoveRange(existing);

                var newEntities = regionIds.Select(rId => new PersonnelRegionPermission
                {
                    PersonnelId = personnelId,
                    RegionId = rId
                });
                await db.PersonnelRegionPermission_Table.AddRangeAsync(newEntities, cancellationToken);

                await scope.CommitAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.ReplaceForPersonnelAsync");
                return Result.Failure(Error.Unexpected("Bölge yetkileri güncellenemedi.", ex, "PersonnelRegionPermission.Replace.Failed"));
            }
        }
    }
}
