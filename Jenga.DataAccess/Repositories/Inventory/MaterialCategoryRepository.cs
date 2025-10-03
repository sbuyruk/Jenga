using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.Inventory;
using Jenga.Models.Inventory;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.Inventory
{
    public class MaterialCategoryRepository : Repository<MaterialCategory>, IMaterialCategoryRepository
    {
        private readonly ApplicationDbContext _db;
        public MaterialCategoryRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<List<MaterialCategory>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _db.MaterialCategory_Table
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<MaterialCategory?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _db.MaterialCategory_Table
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public async Task AddAsync(MaterialCategory category, CancellationToken cancellationToken = default)
        {
            await _db.MaterialCategory_Table.AddAsync(category, cancellationToken);
        }
    }
}