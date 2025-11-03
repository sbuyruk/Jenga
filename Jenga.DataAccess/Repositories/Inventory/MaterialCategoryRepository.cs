using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.Inventory;
using Jenga.Models.Inventory;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Repositories.Inventory
{
    public class MaterialCategoryRepository : Repository<MaterialCategory>, IMaterialCategoryRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        public MaterialCategoryRepository(IDbContextFactory<ApplicationDbContext> dbFactory) : base(dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<bool> AnyAsync(Expression<Func<MaterialCategory, bool>> predicate, CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            return await db.Set<MaterialCategory>().AnyAsync(predicate, cancellationToken);
        }
    }
}