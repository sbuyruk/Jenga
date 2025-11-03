using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.Menu;
using Jenga.Models.Common;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.Menu
{
    // RolMenuRepository updated to accept IDbContextFactory and use short-lived DbContext instances.
    public class RolMenuRepository : Repository<RolMenu>, IRolMenuRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public RolMenuRepository(IDbContextFactory<ApplicationDbContext> dbFactory) : base(dbFactory)
        {
            _dbFactory = dbFactory;
        }

        // If you previously exposed a sync Save(), prefer an async SaveAsync when working with factory-created contexts.
        public async Task SaveAsync(CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            await db.SaveChangesAsync(cancellationToken);
        }

        // Example repository-specific method
        public async Task<IEnumerable<RolMenu>> GetByRolIdAsync(int rolId, CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            return await db.RolMenu_Table
                .AsNoTracking()
                .Where(rm => rm.RolId == rolId)
                .ToListAsync(cancellationToken);
        }
    }
}