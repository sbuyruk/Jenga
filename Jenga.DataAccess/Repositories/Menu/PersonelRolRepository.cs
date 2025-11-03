using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.Menu;
using Jenga.Models.Common;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.Menu
{
    // PersonelRolRepository updated to accept IDbContextFactory and use short-lived DbContext instances.
    public class PersonelRolRepository : Repository<PersonelRol>, IPersonelRolRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public PersonelRolRepository(IDbContextFactory<ApplicationDbContext> dbFactory) : base(dbFactory)
        {
            _dbFactory = dbFactory;
        }

        // If you need an explicit Save/Commit at repository-level, prefer an async variant and use a short-lived context.
        // Example: add or update then commit immediately.
        public async Task SaveAsync(CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            await db.SaveChangesAsync(cancellationToken);
        }

        // Example repository-specific method
        public async Task<IEnumerable<PersonelRol>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            return await db.PersonelRol_Table
                .AsNoTracking()
                .Where(pr => pr.PersonelId == personelId)
                .ToListAsync(cancellationToken);
        }
    }
}