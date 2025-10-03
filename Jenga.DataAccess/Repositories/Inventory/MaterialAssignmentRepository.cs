using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.Inventory;
using Jenga.Models.Inventory;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.Inventory
{
    public class MaterialAssignmentRepository : Repository<MaterialAssignment>, IMaterialAssignmentRepository
    {
        private readonly ApplicationDbContext _db;
        public MaterialAssignmentRepository(ApplicationDbContext db) : base(db) { _db = db; }

        public async Task<List<MaterialAssignment>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _db.MaterialAssignment_Table.ToListAsync(cancellationToken);

        public async Task<MaterialAssignment?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => await _db.MaterialAssignment_Table.AsNoTracking().FirstOrDefaultAsync(ma => ma.Id == id, cancellationToken);

        public async Task AddAsync(MaterialAssignment assignment, CancellationToken cancellationToken = default)
            => await _db.MaterialAssignment_Table.AddAsync(assignment, cancellationToken);
    }
}