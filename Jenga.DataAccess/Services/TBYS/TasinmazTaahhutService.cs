using Jenga.DataAccess.Data;
using Jenga.Models.TBYS;
using Jenga.Utility.Logging;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.TBYS
{
    public class TasinmazTaahhutService : ITasinmazTaahhutService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService _logService;

        public TasinmazTaahhutService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService;
        }

        public async Task<List<TasinmazTaahhut>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.TasinmazTaahhut_Table.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<TasinmazTaahhut?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.TasinmazTaahhut_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<List<TasinmazTaahhut>> GetByTasinmazIdAsync(int tasinmazId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.TasinmazTaahhut_Table.AsNoTracking().Where(x => x.TasinmazId == tasinmazId).ToListAsync(cancellationToken);
        }

        public async Task<List<TasinmazTaahhut>> GetByBagisciIdAsync(int bagisciId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.TasinmazTaahhut_Table.AsNoTracking().Where(x => x.BagisciId == bagisciId).ToListAsync(cancellationToken);
        }

        public async Task<bool> AddAsync(TasinmazTaahhut entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                await db.TasinmazTaahhut_Table.AddAsync(entity, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("TasinmazTaahhutService.AddAsync hata.", ex);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(TasinmazTaahhut entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.TasinmazTaahhut_Table.Update(entity);
                await db.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("TasinmazTaahhutService.UpdateAsync hata.", ex);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.TasinmazTaahhut_Table.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                if (entity == null) return false;

                db.TasinmazTaahhut_Table.Remove(entity);
                await db.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("TasinmazTaahhutService.DeleteAsync hata.", ex);
                throw;
            }
        }
    }
}
