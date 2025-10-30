using Jenga.DataAccess.Repositories.IRepository;
using Jenga.DataAccess.Repositories.IRepository.Inventory;
using Jenga.Models.Inventory;
using Jenga.Utility.Logging;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.Inventory
{
    public class MaterialService : IMaterialService
    {
        private readonly IUnitOfWork _unitOfWork;
        // Material list cache
        private List<Material>? _materialsCache;

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
            await _unitOfWork.Material.SaveChangesAsync(cancellationToken);
            // Invalidate cache if exists
            _materialsCache = null;
            return true;
        }

        public async Task<bool> UpdateAsync(Material material, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.Material.UpdateAsync(material);
            await _unitOfWork.Material.SaveChangesAsync(cancellationToken);
            _materialsCache = null;
            return true;
        }

        public async Task<bool> DeleteAsync(int materialId, CancellationToken cancellationToken = default)
        {
            var hasEntry = await _unitOfWork.MaterialEntry
                .AnyAsync(m => m.MaterialId == materialId);

            if (hasEntry)
                return false;

            var hasExit = await _unitOfWork.MaterialExit
                .AnyAsync(m => m.MaterialId == materialId);

            if (hasExit)
                return false;

            var hasInventory = await _unitOfWork.MaterialInventory
                .AnyAsync(m => m.MaterialId == materialId);

            if (hasInventory)
                return false;

            // Ensure the entity is tracked
            var entity = await _unitOfWork.Material.GetByIdAsync(materialId, cancellationToken);
            if (entity != null)
            {
                _unitOfWork.Material.Remove(entity);
                await _unitOfWork.Material.SaveChangesAsync(cancellationToken);
                return true;
            }
            return false;

        }

        public async Task<bool> AnyAsync(Expression<Func<Material, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var materials = await _unitOfWork.Material.GetAllAsync(cancellationToken);
            return materials.Any(predicate.Compile());
        }

        // Yardımcı Metotlar
        public async Task<string> GetMaterialNameAsync(int materialId, CancellationToken cancellationToken = default)
        {
            if (_materialsCache == null)
                _materialsCache = await GetAllAsync(cancellationToken);
            var material = _materialsCache.FirstOrDefault(x => x.Id == materialId);
            return material?.MaterialName ?? "";
        }

        public async Task<int> GetUnitIdAsync(int materialId, CancellationToken cancellationToken = default)
        {
            if (_materialsCache == null)
                _materialsCache = await GetAllAsync(cancellationToken);
            var material = _materialsCache.FirstOrDefault(x => x.Id == materialId);
            return material?.MaterialUnitId ?? 0;
        }
        public async Task<(bool CanDelete, string? Reason)> CanDeleteAsync(int id)
        {
            if (await _unitOfWork.MaterialEntry.AnyAsync(m => m.MaterialId == id))
                return (false, "Bu malzeme envantere giriş (MaterialEntry) kayıtlarında bulunmaktadır, önce onu silmelisiniz.");

            if (await _unitOfWork.MaterialExit.AnyAsync(m => m.MaterialId == id))
                return (false, "Bu malzeme envanterden çıkış (MaterialExit) kayıtlarında bulunmaktadır, önce onu silmelisiniz.");

            if (await _unitOfWork.MaterialInventory.AnyAsync(m => m.MaterialId == id))
                return (false, "Bu malzeme envanter (MaterialInventory) kayıtlarında bulunmaktadır, önce onu silmelisiniz.");

            return (true, null);
        }
    }
}