using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.Inventory;
using Jenga.Models.Inventory;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.Inventory
{
    public class MaterialExitRepository : Repository<MaterialExit>, IMaterialExitRepository
    {
        private readonly ApplicationDbContext _db;
        public MaterialExitRepository(ApplicationDbContext db) : base(db) { _db = db; }

        public async Task<List<MaterialExit>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _db.MaterialExit_Table.ToListAsync(cancellationToken);

        public async Task<MaterialExit?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => await _db.MaterialExit_Table.AsNoTracking().FirstOrDefaultAsync(me => me.Id == id, cancellationToken);

        public async Task AddAsync(MaterialExit exit, CancellationToken cancellationToken = default)
            => await _db.MaterialExit_Table.AddAsync(exit, cancellationToken);
    }
}