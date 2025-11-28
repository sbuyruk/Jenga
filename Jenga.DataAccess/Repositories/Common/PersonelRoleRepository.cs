using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.Common;
using Jenga.Models.Common;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.Common
{
    // PersonelRolRepository updated to accept IDbContextFactory and use short-lived DbContext instances.
    public class PersonelRoleRepository : Repository<PersonelRole>, IPersonelRoleRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public PersonelRoleRepository(IDbContextFactory<ApplicationDbContext> dbFactory) : base(dbFactory)
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
        public async Task<IEnumerable<PersonelRole>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            return await db.PersonelRol_Table
                .AsNoTracking()
                .Where(pr => pr.PersonelId == personelId)
                .ToListAsync(cancellationToken);
        }
    }
}