using Jenga.DataAccess.Repositories.IRepository;
using Jenga.DataAccess.Repositories.IRepository.Inventory;
using Jenga.Models.Inventory;
using Jenga.Utility.Logging;

namespace Jenga.DataAccess.Services.Inventory
{
    public class MaterialService : IMaterialService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MaterialService(IUnitOfWork unitOfWork, ILogService logService)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<Material>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _unitOfWork.Material.GetAllAsync(cancellationToken);

        public async Task<Material?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => await _unitOfWork.Material.GetByIdAsync(id, cancellationToken);

        public async Task<Material?> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default)
            => await _unitOfWork.Material.GetByIdWithRelationsAsync(id, cancellationToken);

        public async Task<bool> AddAsync(Material material, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.Material.AddAsync(material, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);
            return true;
        }

        public async Task<bool> UpdateAsync(Material material, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.Material.UpdateAsync(material);
            await _unitOfWork.SaveAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAsync(Material material, CancellationToken cancellationToken = default)
        {
            _unitOfWork.Material.Remove(material);
            await _unitOfWork.SaveAsync(cancellationToken);
            return true;
        }
    }
}