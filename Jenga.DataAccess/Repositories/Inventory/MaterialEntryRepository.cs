using System.Linq.Expressions;
using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.Inventory;
using Jenga.Models.Inventory;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.Inventory
{
    public class MaterialEntryRepository : Repository<MaterialEntry>, IMaterialEntryRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        public MaterialEntryRepository(IDbContextFactory<ApplicationDbContext> dbFactory) : base(dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<bool> AnyAsync(Expression<Func<MaterialEntry, bool>> predicate, CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            return await db.MaterialEntry_Table.AnyAsync(predicate, cancellationToken);
        }
    }
}