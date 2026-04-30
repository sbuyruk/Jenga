using Jenga.DataAccess.Data;
using Jenga.Models.Inventory;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.Inventory
{
    public class MaterialAssetLogService : IMaterialAssetLogService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public MaterialAssetLogService(IDbContextFactory<ApplicationDbContext> dbFactory)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        }

        public async Task<List<MaterialAssetLog>> GetByAssetIdAsync(int materialAssetId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.MaterialAssetLog_Table
                .AsNoTracking()
                .Where(l => l.MaterialAssetId == materialAssetId)
                .OrderByDescending(l => l.TransactionDate)
                .ToListAsync(cancellationToken);
        }
    }
}
