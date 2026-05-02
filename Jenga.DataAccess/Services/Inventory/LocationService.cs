using Jenga.DataAccess.Data;
using Jenga.Models.Inventory;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.Inventory
{
    public class LocationService : ILocationService
    {
        private const string Source = nameof(LocationService);

        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService _logService;
        private List<Location>? _locationsCache;

        private readonly IMaterialEntryService _materialEntryService;
        private readonly IMaterialExitService _materialExitService;
        private readonly IMaterialInventoryService _materialInventoryService;

        public LocationService(
            IDbContextFactory<ApplicationDbContext> dbFactory,
            ILogService logService,
            IMaterialEntryService materialEntryService,
            IMaterialExitService materialExitService,
            IMaterialInventoryService materialInventoryService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
            _materialEntryService = materialEntryService;
            _materialExitService = materialExitService;
            _materialInventoryService = materialInventoryService;
        }

        public async Task<Result<List<Location>>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.Location_Table.AsNoTracking().ToListAsync(cancellationToken);
                return Result.Success(list);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.GetAllAsync");
                return Result.Failure<List<Location>>(Error.Unexpected("Konum listesi alinamadi.", ex, "Location.GetAll.Failed"));
            }
        }

        public async Task<Result<Location>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.Location_Table.AsNoTracking().FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
                if (entity is null)
                    return Result.Failure<Location>(Error.NotFound($"Konum bulunamadi (Id={id}).", "Location.NotFound"));
                return Result.Success(entity);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.GetByIdAsync");
                return Result.Failure<Location>(Error.Unexpected("Konum getirilemedi.", ex, "Location.GetById.Failed"));
            }
        }

        public async Task<Result> AddAsync(Location location, CancellationToken cancellationToken = default)
        {
            if (location == null)
                return Result.Failure(Error.Validation("Konum bos olamaz.", "Location.Null"));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                await db.Location_Table.AddAsync(location, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                _locationsCache = null;
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.AddAsync");
                return Result.Failure(Error.Unexpected("Konum eklenemedi.", ex, "Location.Add.Failed"));
            }
        }

        public async Task<Result> UpdateAsync(Location location, CancellationToken cancellationToken = default)
        {
            if (location == null)
                return Result.Failure(Error.Validation("Konum bos olamaz.", "Location.Null"));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.Location_Table.Update(location);
                await db.SaveChangesAsync(cancellationToken);
                _locationsCache = null;
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.UpdateAsync");
                return Result.Failure(Error.Unexpected("Konum güncellenemedi.", ex, "Location.Update.Failed"));
            }
        }

        public async Task<Result> DeleteAsync(Location location, CancellationToken cancellationToken = default)
        {
            if (location == null)
                return Result.Failure(Error.Validation("Konum bos olamaz.", "Location.Null"));

            // Check for dependencies before deleting
            var entryAny = await _materialEntryService.AnyAsync(m => m.LocationId == location.Id);
            if (entryAny.IsFailure) return Result.Failure(entryAny.Error);
            if (entryAny.Value) return Result.Failure(Error.Conflict("Bu konum bir malzeme girisinde kullaniliyor.", "Location.InUse.Entry"));

            var exitAny = await _materialExitService.AnyAsync(m => m.LocationId == location.Id);
            if (exitAny.IsFailure) return Result.Failure(exitAny.Error);
            if (exitAny.Value) return Result.Failure(Error.Conflict("Bu konum bir malzeme çikisinda kullaniliyor.", "Location.InUse.Exit"));

            var invAny = await _materialInventoryService.AnyAsync(m => m.LocationId == location.Id);
            if (invAny.IsFailure) return Result.Failure(invAny.Error);
            if (invAny.Value) return Result.Failure(Error.Conflict("Bu konum bir envanter kaydinda kullaniliyor.", "Location.InUse.Inventory"));

            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.Location_Table.FirstOrDefaultAsync(l => l.Id == location.Id, cancellationToken);
                if (entity == null)
                    return Result.Failure(Error.NotFound($"Konum bulunamadi (Id={location.Id}).", "Location.NotFound"));
                db.Location_Table.Remove(entity);
                await db.SaveChangesAsync(cancellationToken);
                _locationsCache = null;
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.DeleteAsync");
                return Result.Failure(Error.Unexpected("Konum silinemedi.", ex, "Location.Delete.Failed"));
            }
        }

        public async Task<Result<(bool CanDelete, string? Reason)>> CanDeleteAsync(int locationId)
        {
            try
            {
                var selfAny = await AnyAsync(m => m.ParentId == locationId);
                if (selfAny.IsFailure) return Result.Failure<(bool CanDelete, string? Reason)>(selfAny.Error);
                if (selfAny.Value)
                    return Result.Success<(bool CanDelete, string? Reason)>((false, "Bu konumun altinda kayitli konum bulunmaktadir, önce onu silmelisiniz."));

                var entryAny = await _materialEntryService.AnyAsync(m => m.LocationId == locationId);
                if (entryAny.IsFailure) return Result.Failure<(bool CanDelete, string? Reason)>(entryAny.Error);
                if (entryAny.Value)
                    return Result.Success<(bool CanDelete, string? Reason)>((false, "Bu konum bir malzeme girisinde kullaniliyor, önce onu silmelisiniz."));

                var exitAny = await _materialExitService.AnyAsync(m => m.LocationId == locationId);
                if (exitAny.IsFailure) return Result.Failure<(bool CanDelete, string? Reason)>(exitAny.Error);
                if (exitAny.Value)
                    return Result.Success<(bool CanDelete, string? Reason)>((false, "Bu konum bir malzeme çikisinda kullaniliyor, önce onu silmelisiniz."));

                var invAny = await _materialInventoryService.AnyAsync(m => m.LocationId == locationId);
                if (invAny.IsFailure) return Result.Failure<(bool CanDelete, string? Reason)>(invAny.Error);
                if (invAny.Value)
                    return Result.Success<(bool CanDelete, string? Reason)>((false, "Bu konum bir malzeme envanterinde kullaniliyor, önce onu silmelisiniz."));

                return Result.Success<(bool CanDelete, string? Reason)>((true, null));
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.CanDeleteAsync");
                return Result.Failure<(bool CanDelete, string? Reason)>(Error.Unexpected("Konum silinebilirlik kontrolü yapilamadi.", ex, "Location.CanDelete.Failed"));
            }
        }

        // Yardimci Metot: Parent adini döndür
        public async Task<string> GetParentLocationNameAsync(int? parentId, CancellationToken cancellationToken = default)
        {
            if (parentId == null) return "";
            if (_locationsCache == null)
            {
                var allResult = await GetAllAsync(cancellationToken);
                if (allResult.IsFailure) return "";
                _locationsCache = allResult.Value;
            }
            var parent = _locationsCache.FirstOrDefault(x => x.Id == parentId);
            return parent?.LocationName ?? "";
        }

        public async Task<Result<bool>> AnyAsync(Expression<Func<Location, bool>> predicate)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync();
                var any = await db.Location_Table.AsNoTracking().AnyAsync(predicate);
                return Result.Success(any);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.AnyAsync");
                return Result.Failure<bool>(Error.Unexpected("Konum sorgusu yapilamadi.", ex, "Location.Any.Failed"));
            }
        }
    }
}