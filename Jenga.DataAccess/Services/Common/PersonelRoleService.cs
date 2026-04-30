using Jenga.DataAccess.Data;
using Jenga.Models.Common;
using Jenga.Utility.Logging;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.Menu
{
    public class PersonelRoleService : IPersonelRoleService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService _logService;

        public PersonelRoleService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        }

        public async Task<List<PersonelRole>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                return await db.PersonelRol_Table.AsNoTracking().ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logService.LogError("PersonelRoleService.GetAllAsync", ex);
                return new List<PersonelRole>();
            }
        }

        public async Task<PersonelRole?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.PersonelRol_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<PersonelRole>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.PersonelRol_Table
                .AsNoTracking()
                .Where(pr => pr.PersonelId == personelId)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> AddAsync(PersonelRole personelRole, CancellationToken cancellationToken = default)
        {
            if (personelRole == null) throw new ArgumentNullException(nameof(personelRole));

            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                await db.PersonelRol_Table.AddAsync(personelRole, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logService.LogError("PersonelRoleService.AddAsync", ex);
                return false;
            }
        }

        public async Task<bool> UpdateAsync(PersonelRole personelRole, CancellationToken cancellationToken = default)
        {
            if (personelRole == null) throw new ArgumentNullException(nameof(personelRole));

            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.PersonelRol_Table.Update(personelRole);
                await db.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logService.LogError("PersonelRoleService.UpdateAsync", ex);
                return false;
            }
        }

        public async Task<bool> DeleteAsync(PersonelRole personelRole, CancellationToken cancellationToken = default)
        {
            if (personelRole == null) return false;

            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.PersonelRol_Table.Remove(personelRole);
                await db.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logService.LogError("PersonelRoleService.DeleteAsync", ex);
                return false;
            }
        }
    }
}
