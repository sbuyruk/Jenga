using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.Inventory;
using Jenga.Models.Inventory;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.Inventory
{
    public class MaterialRepository : Repository<Material>, IMaterialRepository
    {
        private readonly ApplicationDbContext _db;
        public MaterialRepository(ApplicationDbContext db) : base(db) { _db = db; }

        public async Task<List<Material>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _db.Material_Table.ToListAsync(cancellationToken);

        public async Task<Material?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => await _db.Material_Table.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

        public async Task AddAsync(Material material, CancellationToken cancellationToken = default)
            => await _db.Material_Table.AddAsync(material, cancellationToken);

        // Eğer navigation property ile ilişkili veri çekmek istersen:
        public async Task<Material?> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _db.Material_Table
                //.Include(m => m.Category)
                //.Include(m => m.Brand)
                //.Include(m => m.Model)
                .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        }
    }
}