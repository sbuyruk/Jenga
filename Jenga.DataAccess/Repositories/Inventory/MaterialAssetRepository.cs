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
    public class MaterialAssetRepository : Repository<MaterialAsset>, IMaterialAssetRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public MaterialAssetRepository(IDbContextFactory<ApplicationDbContext> dbFactory) : base(dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<MaterialAsset?> GetBySerialNumberAsync(string serialNumber, CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            return await db.MaterialAsset_Table
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.SerialNumber == serialNumber, cancellationToken);
        }

        public async Task<MaterialAsset?> GetByBarcodeAsync(string barcode, CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            return await db.MaterialAsset_Table
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Barcode == barcode, cancellationToken);
        }

        public async Task<List<MaterialAsset>> GetByMaterialIdAsync(int materialId, CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            return await db.MaterialAsset_Table
                .AsNoTracking()
                .Where(a => a.MaterialId == materialId)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<MaterialAsset>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            return await db.MaterialAsset_Table
                .AsNoTracking()
                .Where(a => a.PersonelId == personelId)
                .ToListAsync(cancellationToken);
        }
    }
}
