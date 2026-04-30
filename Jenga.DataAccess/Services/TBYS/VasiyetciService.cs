using Jenga.DataAccess.Data;
using Jenga.Models.TBYS;
using Jenga.Utility.Logging;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.TBYS
{
    public class VasiyetciService : IVasiyetciService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService _logService;

        public VasiyetciService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService;
        }

        public async Task<List<Vasiyetci>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Vasiyetci_Table.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<Vasiyetci?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Vasiyetci_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<List<Vasiyetci>> GetByTCKimlikAsync(long tcKimlik, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Vasiyetci_Table.AsNoTracking().Where(x => x.TCKimlikNo == tcKimlik).ToListAsync(cancellationToken);
        }

        public async Task<bool> AddAsync(Vasiyetci entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                await db.Vasiyetci_Table.AddAsync(entity, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("VasiyetciService.AddAsync hata.", ex);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(Vasiyetci entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.Vasiyetci_Table.Update(entity);
                await db.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("VasiyetciService.UpdateAsync hata.", ex);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.Vasiyetci_Table.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                if (entity == null) return false;

                db.Vasiyetci_Table.Remove(entity);
                await db.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("VasiyetciService.DeleteAsync hata.", ex);
                throw;
            }
        }

        public async Task<bool> AnyAsync(Expression<Func<Vasiyetci, bool>> predicate, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Vasiyetci_Table.AnyAsync(predicate, cancellationToken);
        }
    }
}
