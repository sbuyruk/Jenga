using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.Menu;
using Jenga.Models.Ortak;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.Menu
{
    // PersonelMenuRepository follows the existing repository pattern (IDbContextFactory + short-lived contexts).
    public class PersonelMenuRepository : Repository<PersonelMenu>, IPersonelMenuRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public PersonelMenuRepository(IDbContextFactory<ApplicationDbContext> dbFactory) : base(dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task SaveAsync(CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            await db.SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<PersonelMenu>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            return await db.PersonelMenuleri
                .AsNoTracking()
                .Where(pm => pm.PersonelId == personelId)
                .ToListAsync(cancellationToken);
        }
    }
}