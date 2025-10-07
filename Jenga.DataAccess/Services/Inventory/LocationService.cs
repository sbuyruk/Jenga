using Jenga.DataAccess.Repositories.IRepository;
using Jenga.DataAccess.Repositories.IRepository.Inventory;
using Jenga.Models.Inventory;

namespace Jenga.DataAccess.Services.Inventory
{
    public class LocationService : ILocationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public LocationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<Location>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _unitOfWork.Location.GetAllAsync(cancellationToken);

        public async Task<Location?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => await _unitOfWork.Location.GetByIdAsync(id, cancellationToken);

        public async Task<bool> AddAsync(Location location, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.Location.AddAsync(location, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);
            return true;
        }

        public async Task<bool> UpdateAsync(Location location, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.Location.UpdateAsync(location);
            await _unitOfWork.SaveAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAsync(Location location, CancellationToken cancellationToken = default)
        {
            _unitOfWork.Location.Remove(location);
            await _unitOfWork.SaveAsync(cancellationToken);
            return true;
        }
    }
}