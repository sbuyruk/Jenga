using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.IKYS;
using Jenga.Models.IKYS;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.IKYS
{
    // PersonelRepository updated to accept IDbContextFactory and use short-lived DbContext instances.
    public class PersonelRepository : Repository<Personel>, IPersonelRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public PersonelRepository(IDbContextFactory<ApplicationDbContext> dbFactory) : base(dbFactory)
        {
            _dbFactory = dbFactory;
        }

        // If you previously had a sync Save() that used a long-lived DbContext, prefer an async SaveAsync when using factory-created contexts.
        public async Task SaveAsync(CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            await db.SaveChangesAsync(cancellationToken);
        }

    }
}