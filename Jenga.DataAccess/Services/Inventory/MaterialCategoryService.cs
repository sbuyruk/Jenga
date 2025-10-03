using Jenga.DataAccess.Repositories.IRepository;
using Jenga.DataAccess.Repositories.IRepository.Inventory;
using Jenga.Models.Inventory;

namespace Jenga.DataAccess.Services.Inventory
{
    public class MaterialCategoryService : IMaterialCategoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MaterialCategoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<MaterialCategory>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _unitOfWork.MaterialCategory.GetAllAsync(cancellationToken);

        public async Task<MaterialCategory?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => await _unitOfWork.MaterialCategory.GetByIdAsync(id, cancellationToken);

        public async Task<bool> AddAsync(MaterialCategory category, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.MaterialCategory.AddAsync(category, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);
            return true;
        }

        public async Task<bool> UpdateAsync(MaterialCategory category, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.MaterialCategory.UpdateAsync(category);
            await _unitOfWork.SaveAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAsync(MaterialCategory category, CancellationToken cancellationToken = default)
        {
            _unitOfWork.MaterialCategory.Remove(category);
            await _unitOfWork.SaveAsync(cancellationToken);
            return true;
        }
    }
}