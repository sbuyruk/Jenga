using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.Inventory;
using Jenga.Models.Inventory;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.Inventory
{
    public class LocationRepository : Repository<Location>, ILocationRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        public LocationRepository(IDbContextFactory<ApplicationDbContext> dbFactory) : base(dbFactory)
        {
            _dbFactory = dbFactory;
        }

        // Örnek: kod ile location arama
        public async Task<Location?> GetByCodeAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            return await db.Location_Table.AsNoTracking().FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
        }
    }
}