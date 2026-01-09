using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.TBYS;
using Jenga.Models.TBYS;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Repositories.TBYS
{
    public class SozlesmeTasinmazRepository : Repository<SozlesmeTasinmaz>, ISozlesmeTasinmazRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public SozlesmeTasinmazRepository(IDbContextFactory<ApplicationDbContext> dbFactory) : base(dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<bool> AnyAsync(Expression<Func<SozlesmeTasinmaz, bool>> predicate, CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            return await db.SozlesmeTasinmaz_Table.AnyAsync(predicate, cancellationToken);
        }

        // Add SozlesmeTasinmaz-specific queries here as needed.
    }
}
