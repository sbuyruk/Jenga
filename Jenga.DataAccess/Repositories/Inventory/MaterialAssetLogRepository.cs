using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.Inventory;
using Jenga.Models.Inventory;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.Inventory
{
    public class MaterialAssetLogRepository : Repository<MaterialAssetLog>, IMaterialAssetLogRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public MaterialAssetLogRepository(IDbContextFactory<ApplicationDbContext> dbFactory) : base(dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<List<MaterialAssetLog>> GetByAssetIdAsync(int materialAssetId, CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            return await db.MaterialAssetLog_Table
                .AsNoTracking()
                .Where(l => l.MaterialAssetId == materialAssetId)
                .OrderByDescending(l => l.TransactionDate)
                .ToListAsync(cancellationToken);
        }
    }
}
