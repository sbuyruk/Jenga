using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.Inventory;
using Jenga.Models.Inventory;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.Inventory
{
    public class LocationRepository : Repository<Location>, ILocationRepository
    {
        private readonly ApplicationDbContext _db;
        public LocationRepository(ApplicationDbContext db) : base(db) { _db = db; }

        public async Task<List<Location>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _db.Location_Table.ToListAsync(cancellationToken);

        public async Task<Location?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => await _db.Location_Table.AsNoTracking().FirstOrDefaultAsync(l => l.Id == id, cancellationToken);

        public async Task AddAsync(Location location, CancellationToken cancellationToken = default)
            => await _db.Location_Table.AddAsync(location, cancellationToken);
    }
}