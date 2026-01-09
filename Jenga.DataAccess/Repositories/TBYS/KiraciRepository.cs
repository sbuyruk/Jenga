using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.TBYS;
using Jenga.Models.TBYS;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Repositories.TBYS
{
    public class KiraciRepository : Repository<Kiraci>, IKiraciRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public KiraciRepository(IDbContextFactory<ApplicationDbContext> dbFactory) : base(dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<bool> AnyAsync(Expression<Func<Kiraci, bool>> predicate, CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            return await db.Kiraci_Table.AnyAsync(predicate, cancellationToken);
        }

        // Additional Kiraci-specific queries can be added here.
    }
}
