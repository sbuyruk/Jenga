using Jenga.DataAccess.Repositories.IRepository;
using Jenga.DataAccess.Repositories.IRepository.Inventory;
using Jenga.Models.Inventory;

namespace Jenga.DataAccess.Services.Inventory
{
    public class MaterialBrandService : IMaterialBrandService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MaterialBrandService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<MaterialBrand>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _unitOfWork.MaterialBrand.GetAllAsync(cancellationToken);

        public async Task<MaterialBrand?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => await _unitOfWork.MaterialBrand.GetByIdAsync(id, cancellationToken);

        public async Task<bool> AddAsync(MaterialBrand brand, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.MaterialBrand.AddAsync(brand, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);
            return true;
        }

        public async Task<bool> UpdateAsync(MaterialBrand brand, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.MaterialBrand.UpdateAsync(brand);
            await _unitOfWork.SaveAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAsync(MaterialBrand brand, CancellationToken cancellationToken = default)
        {
            _unitOfWork.MaterialBrand.Remove(brand);
            await _unitOfWork.SaveAsync(cancellationToken);
            return true;
        }
    }
}