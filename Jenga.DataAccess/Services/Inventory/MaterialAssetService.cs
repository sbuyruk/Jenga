using Jenga.DataAccess.Data;
using Jenga.Models.Inventory;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.Inventory
{
    public class MaterialAssetService : IMaterialAssetService
    {
        private const string Source = nameof(MaterialAssetService);

        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService _logService;

        public MaterialAssetService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService;
        }

        public async Task<Result<List<MaterialAsset>>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.MaterialAsset_Table.AsNoTracking().ToListAsync(cancellationToken);
                return Result.Success(list);
            }
            catch (Exception ex)
            {
                _logService?.LogException(ex, $"{Source}.GetAllAsync");
                return Result.Failure<List<MaterialAsset>>(Error.Unexpected("Asset listesi alınamadı.", ex, "MaterialAsset.GetAll.Failed"));
            }
        }

        public async Task<Result<List<MaterialAsset>>> GetByMaterialIdAsync(int materialId, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.MaterialAsset_Table.AsNoTracking().Where(a => a.MaterialId == materialId).ToListAsync(cancellationToken);
                return Result.Success(list);
            }
            catch (Exception ex)
            {
                _logService?.LogException(ex, $"{Source}.GetByMaterialIdAsync");
                return Result.Failure<List<MaterialAsset>>(Error.Unexpected("Asset listesi alınamadı.", ex, "MaterialAsset.GetByMaterial.Failed"));
            }
        }

        public async Task<Result> UpdateAsync(MaterialAsset asset, CancellationToken cancellationToken = default)
        {
            if (asset == null)
                return Result.Failure(Error.Validation("Asset boş olamaz.", "MaterialAsset.Null"));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.MaterialAsset_Table.Update(asset);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService?.LogException(ex, $"{Source}.UpdateAsync");
                return Result.Failure(Error.Unexpected("Asset güncellenemedi.", ex, "MaterialAsset.Update.Failed"));
            }
        }

        public async Task<Result> DeleteAsync(MaterialAsset asset, CancellationToken cancellationToken = default)
        {
            if (asset == null)
                return Result.Failure(Error.Validation("Asset boş olamaz.", "MaterialAsset.Null"));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.MaterialAsset_Table.Remove(asset);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService?.LogException(ex, $"{Source}.DeleteAsync");
                return Result.Failure(Error.Unexpected("Asset silinemedi.", ex, "MaterialAsset.Delete.Failed"));
            }
        }
    }
}

