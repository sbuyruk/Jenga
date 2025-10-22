using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.Inventory;
using Jenga.Models.Inventory;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.Inventory
{
    public class MaterialInventoryRepository : Repository<MaterialInventory>, IMaterialInventoryRepository
    {
        private readonly ApplicationDbContext _db;
        public MaterialInventoryRepository(ApplicationDbContext db) : base(db) { _db = db; }

        public async Task<MaterialInventory?> GetByMaterialLocationAsync(int materialId, int locationId, int materialUnitId, CancellationToken cancellationToken = default)
        {
            return await _db.MaterialInventory_Table
                .FirstOrDefaultAsync(mi =>
                    mi.MaterialId == materialId &&
                    mi.LocationId == locationId &&
                    mi.MaterialUnitId == materialUnitId,
                    cancellationToken
                );
        }
    }
}