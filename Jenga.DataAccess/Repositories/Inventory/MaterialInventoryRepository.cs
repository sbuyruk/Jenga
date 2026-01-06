using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.Inventory;
using Jenga.Models.Inventory;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.Inventory
{
    public class MaterialInventoryRepository : Repository<MaterialInventory>, IMaterialInventoryRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        public MaterialInventoryRepository(IDbContextFactory<ApplicationDbContext> dbFactory) : base(dbFactory)
        {
            _dbFactory = dbFactory;
        }

        /// <summary>
        /// Finds an inventory row by material + optional location + optional person + optional brand + optional model.
        /// If a nullable parameter is null, matches rows with that column IS NULL.
        /// </summary>
        public async Task<MaterialInventory?> GetByMaterialLocationAsync(int materialId, int? locationId, int? personelId, int? brandId = null, int? modelId = null, CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            return await db.MaterialInventory_Table
                .AsNoTracking()
                .FirstOrDefaultAsync(mi =>
                    mi.MaterialId == materialId &&
                    mi.LocationId == locationId &&
                    mi.PersonelId == personelId &&
                    mi.BrandId == brandId &&
                    mi.ModelId == modelId,
                    cancellationToken
                );
        }
    }
}