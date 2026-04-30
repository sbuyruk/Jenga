using Jenga.DataAccess.Data;
using Jenga.Models.Inventory;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.Inventory
{
    public class MaterialAssetService : IMaterialAssetService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public MaterialAssetService(IDbContextFactory<ApplicationDbContext> dbFactory)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        }

        public async Task<List<MaterialAsset>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.MaterialAsset_Table.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<List<MaterialAsset>> GetByMaterialIdAsync(int materialId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.MaterialAsset_Table.AsNoTracking().Where(a => a.MaterialId == materialId).ToListAsync(cancellationToken);
        }

        public async Task<bool> UpdateAsync(MaterialAsset asset, CancellationToken cancellationToken = default)
        {
            if (asset == null) throw new ArgumentNullException(nameof(asset));
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            db.MaterialAsset_Table.Update(asset);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAsync(MaterialAsset asset, CancellationToken cancellationToken = default)
        {
            if (asset == null) throw new ArgumentNullException(nameof(asset));
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            db.MaterialAsset_Table.Remove(asset);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}

