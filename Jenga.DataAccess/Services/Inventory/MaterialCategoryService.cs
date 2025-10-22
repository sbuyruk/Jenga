using Jenga.DataAccess.Repositories.IRepository;
using Jenga.DataAccess.Repositories.IRepository.Inventory;
using Jenga.Models.Inventory;
using System.Linq.Expressions;

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
            await _unitOfWork.MaterialCategory.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> UpdateAsync(MaterialCategory category, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.MaterialCategory.UpdateAsync(category);
            await _unitOfWork.MaterialCategory.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAsync(MaterialCategory category, CancellationToken cancellationToken = default)
        {
            // Önce alt kategori var mı kontrolü
            var hasChildren = await _unitOfWork.MaterialCategory
                .AnyAsync(m => m.ParentCategoryId == category.Id, cancellationToken);

            if (hasChildren)
                return false; // Silme işlemi başarısız

            _unitOfWork.MaterialCategory.Remove(category);
            await _unitOfWork.MaterialCategory.SaveChangesAsync(cancellationToken);
            return true;
        }
        public async Task<bool> AnyAsync(Expression<Func<MaterialCategory, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var items = await _unitOfWork.MaterialCategory.GetAllAsync(cancellationToken);
            return items.Any(predicate.Compile());
        }
    }
}