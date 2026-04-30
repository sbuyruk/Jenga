using Jenga.DataAccess.Data;
using Jenga.Models.Inventory;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.Inventory
{
    public class MaterialInventoryService : IMaterialInventoryService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public MaterialInventoryService(IDbContextFactory<ApplicationDbContext> dbFactory)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        }

        public async Task<List<MaterialInventory>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.MaterialInventory_Table.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<bool> AnyAsync(Expression<Func<MaterialInventory, bool>> predicate)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            return await db.MaterialInventory_Table.AsNoTracking().AnyAsync(predicate);
        }
    }
}
