using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.Common;
using Jenga.Models.Common;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.Common
{
    // RolRepository updated to use IDbContextFactory and short-lived DbContext instances.
    public class RoleRepository : Repository<Role>, IRoleRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public RoleRepository(IDbContextFactory<ApplicationDbContext> dbFactory) : base(dbFactory)
        {
            _dbFactory = dbFactory;
        }

        // If you want an explicit save on repository level, prefer async and use a short-lived context.
        public async Task SaveAsync(CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            await db.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<Role>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            return await db.Rol_Table
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<Role?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            return await db.Set<Role>()
                           .AsNoTracking()
                           .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        }

        public async Task AddAsync(Role rol, CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            await db.Set<Role>().AddAsync(rol, cancellationToken);
            // Note: caller should call SaveChangesAsync, or uncomment the next line to commit here:
            await db.SaveChangesAsync(cancellationToken);
        }

        public async Task<Role?> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            return await db.Set<Role>()
                .Include(r => r.PersonelRoles)
                    .ThenInclude(pr => pr.Personel)
                .Include(r => r.RoleMenus)
                    .ThenInclude(rm => rm.Menu)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        }
    }
}