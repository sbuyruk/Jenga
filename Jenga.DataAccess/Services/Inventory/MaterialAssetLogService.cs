using Jenga.DataAccess.Data;
using Jenga.Models.Inventory;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.Inventory
{
    public class MaterialAssetLogService : IMaterialAssetLogService
    {
        private const string Source = nameof(MaterialAssetLogService);

        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService _logService;

        public MaterialAssetLogService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService;
        }

        public async Task<Result<List<MaterialAssetLog>>> GetByAssetIdAsync(int materialAssetId, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.MaterialAssetLog_Table
                    .AsNoTracking()
                    .Where(l => l.MaterialAssetId == materialAssetId)
                    .OrderByDescending(l => l.TransactionDate)
                    .ToListAsync(cancellationToken);
                return Result.Success(list);
            }
            catch (Exception ex)
            {
                _logService?.LogException(ex, $"{Source}.GetByAssetIdAsync");
                return Result.Failure<List<MaterialAssetLog>>(Error.Unexpected("Asset log listesi alınamadı.", ex, "MaterialAssetLog.GetByAsset.Failed"));
            }
        }
    }
}
