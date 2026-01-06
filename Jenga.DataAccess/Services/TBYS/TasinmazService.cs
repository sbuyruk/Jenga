using Jenga.DataAccess.Repositories.IRepository;
using Jenga.DataAccess.Repositories.IRepository.TBYS;
using Jenga.Models.TBYS;
using Jenga.Utility.Logging;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.TBYS
{
    public class TasinmazService : ITasinmazService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogService _logService;
        private List<Tasinmaz>? _tasinmazCache;

        public TasinmazService(IUnitOfWork unitOfWork, ILogService logService)
        {
            _unitOfWork = unitOfWork;
            _logService = logService;
        }

        public async Task<List<Tasinmaz>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _unitOfWork.Tasinmaz.GetAllAsync(cancellationToken);

        public async Task<Tasinmaz?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => await _unitOfWork.Tasinmaz.GetByIdAsync(id, cancellationToken);

        public async Task<Tasinmaz?> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default)
            => await _unitOfWork.Tasinmaz.GetByIdWithRelationsAsync(id, cancellationToken);

        public async Task<bool> AddAsync(Tasinmaz tasinmaz, CancellationToken cancellationToken = default)
        {
            if (tasinmaz == null) throw new ArgumentNullException(nameof(tasinmaz));

            // Optional: check unique EmlakSicilNo if provided
            var sicil = (tasinmaz.EmlakSicilNo ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(sicil) && await ExistsByEmlakSicilNoAsync(sicil, null, cancellationToken))
            {
                _logService?.LogWarning($"AddAsync Aynı EmlakSicilNo zaten kayıtlı: '{sicil}'.");
                return false;
            }

            try
            {
                // repository AddAsync may already call SaveChanges; keep consistent behavior
                await _unitOfWork.Tasinmaz.AddAsync(tasinmaz, cancellationToken);
                await _unitOfWork.Tasinmaz.SaveChangesAsync(cancellationToken);
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
                // Update signature in repositories may accept modifiedBy; pass null to keep behavior consistent
                await _unitOfWork.Tasinmaz.UpdateAsync(tasinmaz, null, cancellationToken);
                await _unitOfWork.Tasinmaz.SaveChangesAsync(cancellationToken);
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
            // If you have other repositories that reference Tasinmaz, check them here (similar to MaterialService).
            // For now remove if exists.
            var entity = await _unitOfWork.Tasinmaz.GetByIdAsync(tasinmazId, cancellationToken);
            if (entity != null)
            {
                _unitOfWork.Tasinmaz.Remove(entity);
                await _unitOfWork.Tasinmaz.SaveChangesAsync(cancellationToken);
                _tasinmazCache = null;
                return true;
            }
            return false;
        }

        public async Task<bool> AnyAsync(Expression<Func<Tasinmaz, bool>> predicate, CancellationToken cancellationToken = default)
            => await _unitOfWork.Tasinmaz.AnyAsync(predicate, cancellationToken);

        // Helpers
        public async Task<string> GetEmlakSicilNoAsync(int id, CancellationToken cancellationToken = default)
        {
            if (_tasinmazCache == null)
                _tasinmazCache = await GetAllAsync(cancellationToken);
            var item = _tasinmazCache.FirstOrDefault(x => x.Id == id);
            return item?.EmlakSicilNo ?? string.Empty;
        }

        public async Task<(bool CanDelete, string? Reason)> CanDeleteAsync(int id)
        {
            // Extend this with repository checks if there are FK relations to Tasinmaz_Table.
            return (true, null);
        }

        public async Task<bool> ExistsByEmlakSicilNoAsync(string emlakSicilNo, int? excludeId = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(emlakSicilNo)) return false;
            var normalized = emlakSicilNo.Trim().ToLowerInvariant();

            Expression<Func<Tasinmaz, bool>> predicate = m =>
                m.EmlakSicilNo != null &&
                m.EmlakSicilNo.Trim().ToLower() == normalized &&
                (!excludeId.HasValue || m.Id != excludeId.Value);

            return await _unitOfWork.Tasinmaz.AnyAsync(predicate, cancellationToken);
        }
    }
}
