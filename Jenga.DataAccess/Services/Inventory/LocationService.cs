using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.Inventory;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.Inventory
{
    public class LocationService : ILocationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private List<Location>? _locationsCache;

        private readonly IMaterialEntryService _materialEntryService;
        private readonly IMaterialExitService _materialExitService;
        private readonly IMaterialInventoryService _materialInventoryService;

        public LocationService(
            IUnitOfWork unitOfWork,
             IMaterialEntryService materialEntryService,
            IMaterialExitService materialExitService,
            IMaterialInventoryService materialInventoryService)
        {
            _unitOfWork = unitOfWork;
            _materialEntryService = materialEntryService;
            _materialExitService = materialExitService;
            _materialInventoryService = materialInventoryService;
        }

        public async Task<List<Location>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _unitOfWork.Location.GetAllAsync(cancellationToken);

        public async Task<Location?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => await _unitOfWork.Location.GetByIdAsync(id, cancellationToken);

        public async Task<bool> AddAsync(Location location, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.Location.AddAsync(location, cancellationToken);
            await _unitOfWork.Location.SaveChangesAsync(cancellationToken);
            _locationsCache = null;
            return true;
        }

        public async Task<bool> UpdateAsync(Location location, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.Location.UpdateAsync(location);
            await _unitOfWork.Location.SaveChangesAsync(cancellationToken);
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

            // If no dependencies, proceed with deletion

            _unitOfWork.Location.Remove(location);
            await _unitOfWork.Location.SaveChangesAsync(cancellationToken);
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
        public Task<bool> AnyAsync(Expression<Func<Location, bool>> predicate)
        {
            return _unitOfWork.Location.AnyAsync(predicate);
        }

    }
}