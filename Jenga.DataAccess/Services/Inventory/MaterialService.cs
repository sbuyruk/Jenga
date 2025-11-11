using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.Inventory;
using Jenga.Utility.Logging;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.Inventory
{
    public class MaterialService : IMaterialService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogService _logService;
        // Material list cache
        private List<Material>? _materialsCache;

        public MaterialService(IUnitOfWork unitOfWork, ILogService logService)
        {
            _unitOfWork = unitOfWork;
            _logService = logService;
        }

        public async Task<List<Material>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _unitOfWork.Material.GetAllAsync(cancellationToken);

        public async Task<Material?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => await _unitOfWork.Material.GetByIdAsync(id, cancellationToken);

        public async Task<Material?> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default)
            => await _unitOfWork.Material.GetByIdWithRelationsAsync(id, cancellationToken);

        public async Task<bool> AddAsync(Material material, CancellationToken cancellationToken = default)
        {
            if (material == null) throw new ArgumentNullException(nameof(material));

            var name = (material.MaterialName ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                _logService?.LogWarning("MaterialService.AddAsync Ad boş olmamalı.");
                return false;
            }

            if (await ExistsByNameAsync(name, null, cancellationToken))
            {
                _logService?.LogWarning($"AddAsync Aynı isimde zaten bir malzeme tanımlı: '{name}'.");
                return false;
            }

            try
            {
                material.MaterialName = name;
                await _unitOfWork.Material.AddAsync(material, cancellationToken);
                // repository AddAsync may already call SaveChanges; calling SaveChanges here keeps behavior consistent
                await _unitOfWork.Material.SaveChangesAsync(cancellationToken);
                // Invalidate cache if exists
                _materialsCache = null;
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("Malzeme eklerken hata.", ex);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(Material material, CancellationToken cancellationToken = default)
        {
            if (material == null) throw new ArgumentNullException(nameof(material));

            var name = (material.MaterialName ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                _logService?.LogWarning("MaterialService.UpdateAsync Ad boş olmamalı.");
                return false;
            }

            if (await ExistsByNameAsync(name, material.Id, cancellationToken))
            {
                _logService?.LogWarning($"UpdateAsync Aynı isimde zaten bir malzeme tanımlı: '{name}' (id:{material.Id}).");
                return false;
            }

            try
            {
                material.MaterialName = name;
                // Use repository UpdateAsync signature (modifiedBy optional)
                await _unitOfWork.Material.UpdateAsync(material, null, cancellationToken);
                await _unitOfWork.Material.SaveChangesAsync(cancellationToken);
                _materialsCache = null;
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("Malzeme güncellerken hata", ex);
                throw;
            }
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
                // Repository.Remove already commits; keep consistent pattern and then optionally save again
                _unitOfWork.Material.Remove(entity);
                await _unitOfWork.Material.SaveChangesAsync(cancellationToken);
                _materialsCache = null;
                return true;
            }
            return false;
        }

        public async Task<bool> AnyAsync(Expression<Func<Material, bool>> predicate, CancellationToken cancellationToken = default)
        {
            // Use repository-side AnyAsync to execute in DB
            return await _unitOfWork.Material.AnyAsync(predicate, cancellationToken);
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

        public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            var normalized = name.Trim().ToLowerInvariant();

            Expression<Func<Material, bool>> predicate = m =>
                m.MaterialName != null &&
                m.MaterialName.Trim().ToLower() == normalized &&
                (!excludeId.HasValue || m.Id != excludeId.Value);

            return await _unitOfWork.Material.AnyAsync(predicate, cancellationToken);
        }
    }
}