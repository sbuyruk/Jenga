using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.Inventory;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.Inventory
{
    public class MaterialAssetService : IMaterialAssetService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MaterialAssetService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<MaterialAsset>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _unitOfWork.MaterialAsset.GetAllAsync(cancellationToken);

        public async Task<MaterialAsset?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => await _unitOfWork.MaterialAsset.GetByIdAsync(id, cancellationToken);

        public async Task<MaterialAsset?> GetBySerialNumberAsync(string serialNumber, CancellationToken cancellationToken = default)
            => await _unitOfWork.MaterialAsset.GetBySerialNumberAsync(serialNumber, cancellationToken);

        public async Task<MaterialAsset?> GetByBarcodeAsync(string barcode, CancellationToken cancellationToken = default)
            => await _unitOfWork.MaterialAsset.GetByBarcodeAsync(barcode, cancellationToken);

        public async Task<List<MaterialAsset>> GetByMaterialIdAsync(int materialId, CancellationToken cancellationToken = default)
            => await _unitOfWork.MaterialAsset.GetByMaterialIdAsync(materialId, cancellationToken);

        public async Task<List<MaterialAsset>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default)
            => await _unitOfWork.MaterialAsset.GetByPersonelIdAsync(personelId, cancellationToken);

        public async Task<bool> AddAsync(MaterialAsset asset, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.MaterialAsset.AddAsync(asset, cancellationToken);
            await _unitOfWork.MaterialAsset.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> UpdateAsync(MaterialAsset asset, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.MaterialAsset.UpdateAsync(asset);
            await _unitOfWork.MaterialAsset.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAsync(MaterialAsset asset, CancellationToken cancellationToken = default)
        {
            _unitOfWork.MaterialAsset.Remove(asset);
            await _unitOfWork.MaterialAsset.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> AnyAsync(Expression<Func<MaterialAsset, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var items = await _unitOfWork.MaterialAsset.GetAllAsync(cancellationToken);
            return items.Any(predicate.Compile());
        }
    }
}
