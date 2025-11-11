using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.Inventory;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.Inventory
{
    public class MaterialCategoryService : IMaterialCategoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MaterialCategoryService(
            IUnitOfWork unitOfWork)
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

        public async Task<bool> DeleteAsync(int categoryId, CancellationToken cancellationToken = default)
        {
            var hasChildren = await _unitOfWork.MaterialCategory
                .AnyAsync(m => m.ParentCategoryId == categoryId, cancellationToken);

            if (hasChildren)
                return false;

            // Ensure the entity is tracked
            var entity = await _unitOfWork.MaterialCategory.GetByIdAsync(categoryId, cancellationToken);
            if (entity != null)
            {
                _unitOfWork.MaterialCategory.Remove(entity);
                await _unitOfWork.MaterialCategory.SaveChangesAsync(cancellationToken);
                return true;
            }
            return false;
        }
        public async Task<bool> AnyAsync(Expression<Func<MaterialCategory, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var items = await _unitOfWork.MaterialCategory.GetAllAsync(cancellationToken);
            return items.Any(predicate.Compile());
        }

        public async Task<(bool CanDelete, string? Reason)> CanDeleteAsync(int id)
        {

            if (await AnyAsync(m => m.ParentCategoryId == id))
                return (false, "Bu kategori bir malzemenin üst kategorisi olarak kullanılıyor, önce onu silmelisiniz.");
            if (await _unitOfWork.Material.AnyAsync(m => m.CategoryId == id))
                return (false, "Bu kategori bir malzemenin kategorisi kullanılıyor, önce onu silmelisiniz.");
            if (await _unitOfWork.MaterialCategory.AnyAsync(m => m.ParentCategoryId == id))
                return (false, "Bu kategori bir malzemenin üst kategorisi olarak kullanılıyor, önce onu silmelisiniz.");

            return (true, null);
        }
    }
}