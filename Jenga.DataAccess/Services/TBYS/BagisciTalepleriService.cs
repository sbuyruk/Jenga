using Jenga.DataAccess.Data;
using Jenga.Models.TBYS;
using Jenga.Utility.Logging;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.TBYS
{
    public class BagisciTalepleriService : IBagisciTalepleriService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService _logService;

        public BagisciTalepleriService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService;
        }

        public async Task<List<BagisciTalepleri>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.BagisciTalepleri_Table.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<BagisciTalepleri?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.BagisciTalepleri_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<List<BagisciTalepleri>> GetByBagisciIdAsync(long bagisciId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.BagisciTalepleri_Table.AsNoTracking().Where(x => x.BagisciId == bagisciId).ToListAsync(cancellationToken);
        }

        public async Task<bool> AddAsync(BagisciTalepleri entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                await db.BagisciTalepleri_Table.AddAsync(entity, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("BagisciTalepleriService.AddAsync hata.", ex);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(BagisciTalepleri entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.BagisciTalepleri_Table.Update(entity);
                await db.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("BagisciTalepleriService.UpdateAsync hata.", ex);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.BagisciTalepleri_Table.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                if (entity == null) return false;

                db.BagisciTalepleri_Table.Remove(entity);
                await db.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("BagisciTalepleriService.DeleteAsync hata.", ex);
                throw;
            }
        }

        public async Task<bool> AnyAsync(Expression<Func<BagisciTalepleri, bool>> predicate, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.BagisciTalepleri_Table.AnyAsync(predicate, cancellationToken);
        }
    }
}
