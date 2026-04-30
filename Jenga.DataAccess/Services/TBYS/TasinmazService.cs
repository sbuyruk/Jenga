using Jenga.DataAccess.Data;
using Jenga.Models.TBYS;
using Jenga.Utility.Logging;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.TBYS
{
    public class TasinmazService : ITasinmazService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService _logService;
        private List<Tasinmaz>? _tasinmazCache;

        public TasinmazService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService;
        }

        public async Task<List<Tasinmaz>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Tasinmaz_Table.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<List<Tasinmaz>> GetByEnvanterDurumuAsync(int envanterdeMi, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Tasinmaz_Table.AsNoTracking().Where(x => x.EnvanterdeMi == envanterdeMi).ToListAsync(cancellationToken);
        }

        public Task<List<Tasinmaz>> GetEnvanterdekilerAsync(CancellationToken cancellationToken = default)
            => GetByEnvanterDurumuAsync(1, cancellationToken);

        public async Task<Tasinmaz?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Tasinmaz_Table.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        }

        public async Task<Tasinmaz?> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Tasinmaz_Table.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        }

        public async Task<bool> AddAsync(Tasinmaz tasinmaz, CancellationToken cancellationToken = default)
        {
            if (tasinmaz == null) throw new ArgumentNullException(nameof(tasinmaz));

            var sicil = (tasinmaz.EmlakSicilNo ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(sicil) && await ExistsByEmlakSicilNoAsync(sicil, null, cancellationToken))
            {
                _logService?.LogWarning($"AddAsync Aynı EmlakSicilNo zaten kayıtlı: '{sicil}'.");
                return false;
            }

            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                await db.Tasinmaz_Table.AddAsync(tasinmaz, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                _tasinmazCache = null;
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("Tasinmaz eklerken hata.", ex);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(Tasinmaz tasinmaz, CancellationToken cancellationToken = default)
        {
            if (tasinmaz == null) throw new ArgumentNullException(nameof(tasinmaz));

            var sicil = (tasinmaz.EmlakSicilNo ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(sicil) && await ExistsByEmlakSicilNoAsync(sicil, tasinmaz.Id, cancellationToken))
            {
                _logService?.LogWarning($"UpdateAsync Aynı EmlakSicilNo zaten kayıtlı: '{sicil}' (id:{tasinmaz.Id}).");
                return false;
            }

            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.Tasinmaz_Table.Update(tasinmaz);
                await db.SaveChangesAsync(cancellationToken);
                _tasinmazCache = null;
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("Tasinmaz güncellerken hata.", ex);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int tasinmazId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var entity = await db.Tasinmaz_Table.FirstOrDefaultAsync(x => x.Id == tasinmazId, cancellationToken);
            if (entity == null) return false;

            db.Tasinmaz_Table.Remove(entity);
            await db.SaveChangesAsync(cancellationToken);
            _tasinmazCache = null;
            return true;
        }

        public async Task<bool> AnyAsync(Expression<Func<Tasinmaz, bool>> predicate, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Tasinmaz_Table.AnyAsync(predicate, cancellationToken);
        }

        public async Task<string> GetEmlakSicilNoAsync(int id, CancellationToken cancellationToken = default)
        {
            if (_tasinmazCache == null)
                _tasinmazCache = await GetAllAsync(cancellationToken);

            var item = _tasinmazCache.FirstOrDefault(x => x.Id == id);
            return item?.EmlakSicilNo ?? string.Empty;
        }

        public async Task<(bool CanDelete, string? Reason)> CanDeleteAsync(int id)
        {
            await Task.CompletedTask;
            return (true, null);
        }

        public async Task<bool> ExistsByEmlakSicilNoAsync(string emlakSicilNo, int? excludeId = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(emlakSicilNo)) return false;
            var normalized = emlakSicilNo.Trim().ToLowerInvariant();

            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Tasinmaz_Table.AsNoTracking().AnyAsync(m =>
                m.EmlakSicilNo != null &&
                m.EmlakSicilNo.Trim().ToLower() == normalized &&
                (!excludeId.HasValue || m.Id != excludeId.Value), cancellationToken);
        }
    }
}
