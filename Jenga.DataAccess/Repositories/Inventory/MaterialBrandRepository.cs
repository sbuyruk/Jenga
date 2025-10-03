using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.Inventory;
using Jenga.Models.Inventory;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.Inventory
{
    public class MaterialBrandRepository : Repository<MaterialBrand>, IMaterialBrandRepository
    {
        private readonly ApplicationDbContext _db;
        public MaterialBrandRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<List<MaterialBrand>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _db.MaterialBrand_Table.ToListAsync(cancellationToken);
        }

        public async Task<MaterialBrand?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _db.MaterialBrand_Table
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        }

        public async Task AddAsync(MaterialBrand brand, CancellationToken cancellationToken = default)
        {
            await _db.MaterialBrand_Table.AddAsync(brand, cancellationToken);
        }
    }
}