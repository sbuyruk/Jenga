using Jenga.DataAccess.Data;
using Jenga.Models.TBYS;
using Jenga.Utility.Logging;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.TBYS
{
    public class TasinmazBagisciService : ITasinmazBagisciService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService _logService;

        public TasinmazBagisciService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService;
        }

        public async Task<List<TasinmazBagisci>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.TasinmazBagisci_Table.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<TasinmazBagisci?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.TasinmazBagisci_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<TasinmazBagisci?> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.TasinmazBagisci_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<bool> AddAsync(TasinmazBagisci bagisci, CancellationToken cancellationToken = default)
        {
            if (bagisci == null) throw new ArgumentNullException(nameof(bagisci));

            var name = $"{(bagisci.Adi ?? string.Empty).Trim()} {(bagisci.Soyadi ?? string.Empty).Trim()}".Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                _logService?.LogWarning("TasinmazBagisciService.AddAsync Ad/Soyad boş olmamalı.");
                return false;
            }

            if (await ExistsByTCKimlikAsync(bagisci.TCKimlikNo, null, cancellationToken))
            {
                _logService?.LogWarning($"AddAsync Aynı TCKimlikNo zaten kayıtlı: '{bagisci.TCKimlikNo}'.");
                return false;
            }

            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                await db.TasinmazBagisci_Table.AddAsync(bagisci, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("Tasinmaz bagisci eklerken hata.", ex);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(TasinmazBagisci bagisci, CancellationToken cancellationToken = default)
        {
            if (bagisci == null) throw new ArgumentNullException(nameof(bagisci));

            var name = $"{(bagisci.Adi ?? string.Empty).Trim()} {(bagisci.Soyadi ?? string.Empty).Trim()}".Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                _logService?.LogWarning("TasinmazBagisciService.UpdateAsync Ad/Soyad boş olmamalı.");
                return false;
            }

            if (await ExistsByTCKimlikAsync(bagisci.TCKimlikNo, bagisci.Id, cancellationToken))
            {
                _logService?.LogWarning($"UpdateAsync Aynı TCKimlikNo zaten kayıtlı: '{bagisci.TCKimlikNo}' (id:{bagisci.Id}).");
                return false;
            }

            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.TasinmazBagisci_Table.Update(bagisci);
                await db.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("Tasinmaz bagisci güncellerken hata", ex);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int bagisciId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

            // Prevent delete if any Tasinmaz references this bagisci
            if (await db.Tasinmaz_Table.AsNoTracking().AnyAsync(t => t.BagisciId == bagisciId, cancellationToken))
                return false;

            var entity = await db.TasinmazBagisci_Table.FirstOrDefaultAsync(x => x.Id == bagisciId, cancellationToken);
            if (entity == null) return false;

            db.TasinmazBagisci_Table.Remove(entity);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> AnyAsync(Expression<Func<TasinmazBagisci, bool>> predicate, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.TasinmazBagisci_Table.AnyAsync(predicate, cancellationToken);
        }

        public async Task<(bool CanDelete, string? Reason)> CanDeleteAsync(int id)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            if (await db.Tasinmaz_Table.AsNoTracking().AnyAsync(t => t.BagisciId == id))
                return (false, "Bu bağışçı bir taşınmaz kaydında referans olarak kullanılıyor, önce onu kaldırmalısınız.");

            return (true, null);
        }

        public async Task<bool> ExistsByTCKimlikAsync(long? tckimlik, int? excludeId = null, CancellationToken cancellationToken = default)
        {
            if (!tckimlik.HasValue || tckimlik.Value == 0) return false;

            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.TasinmazBagisci_Table.AsNoTracking().AnyAsync(b =>
                b.TCKimlikNo.HasValue &&
                b.TCKimlikNo.Value == tckimlik.Value &&
                (!excludeId.HasValue || b.Id != excludeId.Value), cancellationToken);
        }
    }
}
