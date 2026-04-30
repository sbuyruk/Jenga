using Jenga.DataAccess.Data;
using Jenga.Models.TBYS;
using Jenga.Utility.Logging;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.TBYS
{
    public class SozlesmeTasinmazService : ISozlesmeTasinmazService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService _logService;

        public SozlesmeTasinmazService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService;
        }

        public async Task<List<SozlesmeTasinmaz>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.SozlesmeTasinmaz_Table.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<SozlesmeTasinmaz?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.SozlesmeTasinmaz_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<bool> AddAsync(SozlesmeTasinmaz entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            if (!entity.SozlesmeId.HasValue && !entity.TasinmazId.HasValue)
            {
                _logService?.LogWarning("SozlesmeTasinmazService.AddAsync: SozlesmeId veya TasinmazId gerekli.");
                return false;
            }

            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                await db.SozlesmeTasinmaz_Table.AddAsync(entity, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("SozlesmeTasinmaz eklerken hata.", ex);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(SozlesmeTasinmaz entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            if (!entity.SozlesmeId.HasValue && !entity.TasinmazId.HasValue)
            {
                _logService?.LogWarning("SozlesmeTasinmazService.UpdateAsync: SozlesmeId veya TasinmazId gerekli.");
                return false;
            }

            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.SozlesmeTasinmaz_Table.Update(entity);
                await db.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("SozlesmeTasinmaz güncellerken hata.", ex);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var entity = await db.SozlesmeTasinmaz_Table.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (entity == null) return false;

            db.SozlesmeTasinmaz_Table.Remove(entity);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> AnyAsync(Expression<Func<SozlesmeTasinmaz, bool>> predicate, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.SozlesmeTasinmaz_Table.AnyAsync(predicate, cancellationToken);
        }
    }
}
