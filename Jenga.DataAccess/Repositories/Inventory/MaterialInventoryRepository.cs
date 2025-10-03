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

        public async Task<List<MaterialInventory>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _db.MaterialInventory_Table.ToListAsync(cancellationToken);

        public async Task<MaterialInventory?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => await _db.MaterialInventory_Table.AsNoTracking().FirstOrDefaultAsync(mi => mi.Id == id, cancellationToken);

        public async Task AddAsync(MaterialInventory inventory, CancellationToken cancellationToken = default)
            => await _db.MaterialInventory_Table.AddAsync(inventory, cancellationToken);
    }
}