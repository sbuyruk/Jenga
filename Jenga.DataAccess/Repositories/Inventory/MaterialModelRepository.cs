using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.Inventory;
using Jenga.Models.Inventory;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.Inventory
{
    public class MaterialModelRepository : Repository<MaterialModel>, IMaterialModelRepository
    {
        private readonly ApplicationDbContext _db;
        public MaterialModelRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<List<MaterialModel>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _db.MaterialModel_Table.ToListAsync(cancellationToken);
        }

        public async Task<MaterialModel?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _db.MaterialModel_Table
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        }

        public async Task AddAsync(MaterialModel model, CancellationToken cancellationToken = default)
        {
            await _db.MaterialModel_Table.AddAsync(model, cancellationToken);
        }
    }
}