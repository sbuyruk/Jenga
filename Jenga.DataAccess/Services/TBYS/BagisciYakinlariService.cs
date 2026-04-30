using Jenga.DataAccess.Data;
using Jenga.Models.TBYS;
using Jenga.Utility.Logging;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.TBYS
{
    public class BagisciYakinlariService : IBagisciYakinlariService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService _logService;

        public BagisciYakinlariService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService;
        }

        public async Task<List<BagisciYakinlari>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.BagisciYakinlari_Table.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<BagisciYakinlari?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.BagisciYakinlari_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<List<BagisciYakinlari>> GetByBagisciIdAsync(long bagisciId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.BagisciYakinlari_Table.AsNoTracking()
                .Where(x => x.BagisciId == bagisciId)
                .OrderBy(x => x.Sira)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> AddAsync(BagisciYakinlari entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                await db.BagisciYakinlari_Table.AddAsync(entity, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("BagisciYakinlariService.AddAsync hata.", ex);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(BagisciYakinlari entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.BagisciYakinlari_Table.Update(entity);
                await db.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("BagisciYakinlariService.UpdateAsync hata.", ex);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.BagisciYakinlari_Table.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                if (entity == null) return false;

                db.BagisciYakinlari_Table.Remove(entity);
                await db.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("BagisciYakinlariService.DeleteAsync hata.", ex);
                throw;
            }
        }

        public async Task<bool> AnyAsync(Expression<Func<BagisciYakinlari, bool>> predicate, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.BagisciYakinlari_Table.AnyAsync(predicate, cancellationToken);
        }
    }
}
