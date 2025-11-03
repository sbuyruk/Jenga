using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.Menu;
using Jenga.Models.Common;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.Menu
{
    // RolRepository updated to use IDbContextFactory and short-lived DbContext instances.
    public class RolRepository : Repository<Rol>, IRolRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public RolRepository(IDbContextFactory<ApplicationDbContext> dbFactory) : base(dbFactory)
        {
            _dbFactory = dbFactory;
        }

        // If you want an explicit save on repository level, prefer async and use a short-lived context.
        public async Task SaveAsync(CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            await db.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<Rol>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            return await db.Rol_Table
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<Rol?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            return await db.Set<Rol>()
                           .AsNoTracking()
                           .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        }

        public async Task AddAsync(Rol rol, CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            await db.Set<Rol>().AddAsync(rol, cancellationToken);
            // Note: caller should call SaveChangesAsync, or uncomment the next line to commit here:
            // await db.SaveChangesAsync(cancellationToken);
        }

        public async Task<Rol?> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            return await db.Set<Rol>()
                .Include(r => r.PersonelRoller)
                    .ThenInclude(pr => pr.Personel)
                .Include(r => r.RolMenuleri)
                    .ThenInclude(rm => rm.Menu)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        }
    }
}