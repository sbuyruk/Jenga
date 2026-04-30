using Jenga.DataAccess.Data;
using Jenga.Models.Inventory;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.Inventory
{
    public class LocationService : ILocationService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private List<Location>? _locationsCache;

        private readonly IMaterialEntryService _materialEntryService;
        private readonly IMaterialExitService _materialExitService;
        private readonly IMaterialInventoryService _materialInventoryService;

        public LocationService(
            IDbContextFactory<ApplicationDbContext> dbFactory,
            IMaterialEntryService materialEntryService,
            IMaterialExitService materialExitService,
            IMaterialInventoryService materialInventoryService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _materialEntryService = materialEntryService;
            _materialExitService = materialExitService;
            _materialInventoryService = materialInventoryService;
        }

        public async Task<List<Location>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Location_Table.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<Location?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Location_Table.AsNoTracking().FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
        }

        public async Task<bool> AddAsync(Location location, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            await db.Location_Table.AddAsync(location, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            _locationsCache = null;
            return true;
        }

        public async Task<bool> UpdateAsync(Location location, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            db.Location_Table.Update(location);
            await db.SaveChangesAsync(cancellationToken);
            _locationsCache = null;
            return true;
        }

        public async Task<bool> DeleteAsync(Location location, CancellationToken cancellationToken = default)
        {
            // Check for dependencies before deleting
            if (await _materialEntryService.AnyAsync(m => m.LocationId == location.Id))
                return false;
            if (await _materialExitService.AnyAsync(m => m.LocationId == location.Id))
                return false;
            if (await _materialInventoryService.AnyAsync(m => m.LocationId == location.Id))
                return false;

            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var entity = await db.Location_Table.FirstOrDefaultAsync(l => l.Id == location.Id, cancellationToken);
            if (entity == null) return false;
            db.Location_Table.Remove(entity);
            await db.SaveChangesAsync(cancellationToken);
            _locationsCache = null;
            return true;
        }

        public async Task<(bool CanDelete, string? Reason)> CanDeleteAsync(int locationId)
        {
            if (await AnyAsync(m => m.ParentId == locationId))
                return (false, "Bu konumun altında kayıtlı konum bulunmaktadır, önce onu silmelisiniz.");
            if (await _materialEntryService.AnyAsync(m => m.LocationId == locationId))
                return (false, "Bu konum bir malzeme girişinde kullanılıyor, önce onu silmelisiniz.");
            if (await _materialExitService.AnyAsync(m => m.LocationId == locationId))
                return (false, "Bu konum bir malzeme çıkışında kullanılıyor, önce onu silmelisiniz.");
            if (await _materialInventoryService.AnyAsync(m => m.LocationId == locationId))
                return (false, "Bu konum bir malzeme envanterinde kullanılıyor, önce onu silmelisiniz.");
            return (true, null);
        }

        // Yardımcı Metot: Parent adını döndür
        public async Task<string> GetParentLocationNameAsync(int? parentId, CancellationToken cancellationToken = default)
        {
            if (parentId == null) return "";
            if (_locationsCache == null)
                _locationsCache = await GetAllAsync(cancellationToken);
            var parent = _locationsCache.FirstOrDefault(x => x.Id == parentId);
            return parent?.LocationName ?? "";
        }

        public async Task<bool> AnyAsync(Expression<Func<Location, bool>> predicate)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            return await db.Location_Table.AsNoTracking().AnyAsync(predicate);
        }
    }
}