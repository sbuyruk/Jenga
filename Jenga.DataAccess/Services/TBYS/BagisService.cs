using Jenga.DataAccess.Data;
using Jenga.Models.TBYS;
using Jenga.Utility.Logging;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.TBYS
{
    public class BagisService : IBagisService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService _logService;

        public BagisService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService;
        }

        public async Task<List<Bagis>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Bagis_Table.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<Bagis?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Bagis_Table.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        }

        public async Task<Bagis?> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Bagis_Table.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        }

        public async Task<List<Bagis>> GetByBagisciIdAsync(int bagisciId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Bagis_Table.AsNoTracking().Where(b => b.BagisciId == bagisciId).ToListAsync(cancellationToken);
        }

        public async Task<List<Bagis>> GetByTasinmazIdAsync(int tasinmazId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Bagis_Table.AsNoTracking().Where(b => b.TasinmazId == tasinmazId).ToListAsync(cancellationToken);
        }

        public async Task<bool> AddAsync(Bagis bagis, CancellationToken cancellationToken = default)
        {
            if (bagis == null) throw new ArgumentNullException(nameof(bagis));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                await db.Bagis_Table.AddAsync(bagis, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("BagisService.AddAsync hata.", ex);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(Bagis bagis, CancellationToken cancellationToken = default)
        {
            if (bagis == null) throw new ArgumentNullException(nameof(bagis));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.Bagis_Table.Update(bagis);
                await db.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("BagisService.UpdateAsync hata.", ex);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int bagisId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var entity = await db.Bagis_Table.FirstOrDefaultAsync(b => b.Id == bagisId, cancellationToken);
            if (entity == null) return false;

            db.Bagis_Table.Remove(entity);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> AnyAsync(Expression<Func<Bagis, bool>> predicate, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Bagis_Table.AnyAsync(predicate, cancellationToken);
        }

        public async Task<(bool CanDelete, string? Reason)> CanDeleteAsync(int id)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            var exists = await db.Bagis_Table.AsNoTracking().AnyAsync(b => b.Id == id);
            if (!exists) return (false, "Kayıt bulunamadı.");
            return (true, null);
        }
    }
}
