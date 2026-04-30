using Jenga.DataAccess.Data;
using Jenga.Models.Inventory;
using Jenga.Utility.Logging;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.Inventory
{
    public class MaterialService : IMaterialService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService _logService;
        private List<Material>? _materialsCache;

        public MaterialService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService;
        }

        public async Task<List<Material>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Material_Table.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<Material?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Material_Table.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        }

        public async Task<Material?> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Material_Table.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        }

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
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                await db.Material_Table.AddAsync(material, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
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
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.Material_Table.Update(material);
                await db.SaveChangesAsync(cancellationToken);
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
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

            if (await db.MaterialEntry_Table.AsNoTracking().AnyAsync(m => m.MaterialId == materialId, cancellationToken))
                return false;
            if (await db.MaterialExit_Table.AsNoTracking().AnyAsync(m => m.MaterialId == materialId, cancellationToken))
                return false;
            if (await db.MaterialInventory_Table.AsNoTracking().AnyAsync(m => m.MaterialId == materialId, cancellationToken))
                return false;

            var entity = await db.Material_Table.FirstOrDefaultAsync(m => m.Id == materialId, cancellationToken);
            if (entity != null)
            {
                db.Material_Table.Remove(entity);
                await db.SaveChangesAsync(cancellationToken);
                _materialsCache = null;
                return true;
            }
            return false;
        }

        public async Task<bool> AnyAsync(Expression<Func<Material, bool>> predicate, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Material_Table.AsNoTracking().AnyAsync(predicate, cancellationToken);
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
            await using var db = await _dbFactory.CreateDbContextAsync();

            if (await db.MaterialEntry_Table.AsNoTracking().AnyAsync(m => m.MaterialId == id))
                return (false, "Bu malzeme envantere giriş (MaterialEntry) kayıtlarında bulunmaktadır, önce onu silmelisiniz.");

            if (await db.MaterialExit_Table.AsNoTracking().AnyAsync(m => m.MaterialId == id))
                return (false, "Bu malzeme envanterden çıkış (MaterialExit) kayıtlarında bulunmaktadır, önce onu silmelisiniz.");

            if (await db.MaterialInventory_Table.AsNoTracking().AnyAsync(m => m.MaterialId == id))
                return (false, "Bu malzeme envanter (MaterialInventory) kayıtlarında bulunmaktadır, önce onu silmelisiniz.");

            return (true, null);
        }

        public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            var normalized = name.Trim().ToLower();

            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Material_Table.AsNoTracking().AnyAsync(m =>
                m.MaterialName != null &&
                m.MaterialName.Trim().ToLower() == normalized &&
                (!excludeId.HasValue || m.Id != excludeId.Value),
                cancellationToken);
        }
    }
}