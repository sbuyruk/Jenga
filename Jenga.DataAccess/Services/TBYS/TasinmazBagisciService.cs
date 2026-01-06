using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.TBYS;
using Jenga.Utility.Logging;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.TBYS
{
    public class TasinmazBagisciService : ITasinmazBagisciService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogService _logService;
        private List<TasinmazBagisci>? _cache;

        public TasinmazBagisciService(IUnitOfWork unitOfWork, ILogService logService)
        {
            _unitOfWork = unitOfWork;
            _logService = logService;
        }

        public async Task<List<TasinmazBagisci>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _unitOfWork.TasinmazBagisci.GetAllAsync(cancellationToken);

        public async Task<TasinmazBagisci?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => await _unitOfWork.TasinmazBagisci.GetByIdAsync(id, cancellationToken);

        public async Task<TasinmazBagisci?> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default)
            => await _unitOfWork.TasinmazBagisci.GetByIdWithRelationsAsync(id, cancellationToken);

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
                await _unitOfWork.TasinmazBagisci.AddAsync(bagisci, cancellationToken);
                await _unitOfWork.TasinmazBagisci.SaveChangesAsync(cancellationToken);
                _cache = null;
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
                await _unitOfWork.TasinmazBagisci.UpdateAsync(bagisci, null, cancellationToken);
                await _unitOfWork.TasinmazBagisci.SaveChangesAsync(cancellationToken);
                _cache = null;
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
            // Prevent delete if any Tasinmaz references this bagisci
            if (await _unitOfWork.Tasinmaz.AnyAsync(t => t.BagisciId == bagisciId, cancellationToken))
                return false;

            var entity = await _unitOfWork.TasinmazBagisci.GetByIdAsync(bagisciId, cancellationToken);
            if (entity != null)
            {
                _unitOfWork.TasinmazBagisci.Remove(entity);
                await _unitOfWork.TasinmazBagisci.SaveChangesAsync(cancellationToken);
                _cache = null;
                return true;
            }
            return false;
        }

        public async Task<bool> AnyAsync(Expression<Func<TasinmazBagisci, bool>> predicate, CancellationToken cancellationToken = default)
            => await _unitOfWork.TasinmazBagisci.AnyAsync(predicate, cancellationToken);

        public async Task<(bool CanDelete, string? Reason)> CanDeleteAsync(int id)
        {
            if (await _unitOfWork.Tasinmaz.AnyAsync(t => t.BagisciId == id))
                return (false, "Bu bağışçı bir taşınmaz kaydında referans olarak kullanılıyor, önce onu kaldırmalısınız.");

            return (true, null);
        }

        public async Task<bool> ExistsByTCKimlikAsync(long? tckimlik, int? excludeId = null, CancellationToken cancellationToken = default)
        {
            if (!tckimlik.HasValue || tckimlik.Value == 0) return false;

            Expression<Func<TasinmazBagisci, bool>> predicate = b =>
                b.TCKimlikNo.HasValue &&
                b.TCKimlikNo.Value == tckimlik.Value &&
                (!excludeId.HasValue || b.Id != excludeId.Value);

            return await _unitOfWork.TasinmazBagisci.AnyAsync(predicate, cancellationToken);
        }
    }
}