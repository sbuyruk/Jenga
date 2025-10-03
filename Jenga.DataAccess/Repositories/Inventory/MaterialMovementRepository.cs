using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.Inventory;
using Jenga.Models.Inventory;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.Inventory
{
    public class MaterialMovementRepository : Repository<MaterialMovement>, IMaterialMovementRepository
    {
        private readonly ApplicationDbContext _db;
        public MaterialMovementRepository(ApplicationDbContext db) : base(db) { _db = db; }

        public async Task<List<MaterialMovement>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _db.MaterialMovement_Table.ToListAsync(cancellationToken);

        public async Task<MaterialMovement?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => await _db.MaterialMovement_Table.AsNoTracking().FirstOrDefaultAsync(mm => mm.Id == id, cancellationToken);

        public async Task AddAsync(MaterialMovement movement, CancellationToken cancellationToken = default)
            => await _db.MaterialMovement_Table.AddAsync(movement, cancellationToken);
    }
}