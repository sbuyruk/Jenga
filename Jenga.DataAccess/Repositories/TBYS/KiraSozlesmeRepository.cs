using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.TBYS;
using Jenga.Models.TBYS;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Repositories.TBYS
{
    public class KiraSozlesmeRepository : Repository<KiraSozlesme>, IKiraSozlesmeRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public KiraSozlesmeRepository(IDbContextFactory<ApplicationDbContext> dbFactory) : base(dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<bool> AnyAsync(Expression<Func<KiraSozlesme, bool>> predicate, CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            return await db.KiraSozlesme_Table.AnyAsync(predicate, cancellationToken);
        }

        // KiraSozlesme-specific queries can be added here.
    }
}
