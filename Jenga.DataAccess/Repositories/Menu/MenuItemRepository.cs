using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.Menu;
using Jenga.Models.Common;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.Menu
{
    // MenuItemRepository now accepts IDbContextFactory and delegates common operations to base Repository<T>.
    // Use the factory directly for any repository-specific operations that need a short-lived DbContext.
    public class MenuItemRepository : Repository<MenuItem>, IMenuItemRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public MenuItemRepository(IDbContextFactory<ApplicationDbContext> dbFactory) : base(dbFactory)
        {
            _dbFactory = dbFactory;
        }

        // Example of a repository-specific method using the factory to create a short-lived context.
        public async Task<IEnumerable<MenuItem>> GetActiveMenuItemsAsync(CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            return await db.MenuItem_Table
                .AsNoTracking()
                .Where(m => m.IsActive) // assuming MenuItem has IsActive property
                .ToListAsync(cancellationToken);
        }

        // Another example: get menu items including related entities if needed
        public async Task<IEnumerable<MenuItem>> GetMenuItemsWithChildrenAsync(CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            return await db.MenuItem_Table
                .AsNoTracking()
                .Include(m => m.Children) // adjust navigation property name as in your model
                .ToListAsync(cancellationToken);
        }
    }
}