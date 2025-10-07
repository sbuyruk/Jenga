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