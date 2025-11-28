using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.Common;
using Jenga.Models.Common;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.Common
{
    // RolMenuRepository updated to accept IDbContextFactory and use short-lived DbContext instances.
    public class RoleMenuRepository : Repository<RoleMenu>, IRoleMenuRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public RoleMenuRepository(IDbContextFactory<ApplicationDbContext> dbFactory) : base(dbFactory)
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
        public async Task<IEnumerable<RoleMenu>> GetByRolIdAsync(int rolId, CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            return await db.RolMenu_Table
                .AsNoTracking()
                .Where(rm => rm.RoleId == rolId)
                .ToListAsync(cancellationToken);
        }
    }
}