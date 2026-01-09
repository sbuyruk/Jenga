using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.TBYS;
using Jenga.Utility.Logging;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.TBYS
{
    public class KiraSozlesmeService : IKiraSozlesmeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogService _logService;

        public KiraSozlesmeService(IUnitOfWork unitOfWork, ILogService logService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logService = logService;
        }

        public async Task<List<KiraSozlesme>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _unitOfWork.KiraSozlesme.GetAllAsync(cancellationToken);

        public async Task<KiraSozlesme?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => await _unitOfWork.KiraSozlesme.GetByIdAsync(id, cancellationToken);

        public async Task<bool> AddAsync(KiraSozlesme sozlesme, CancellationToken cancellationToken = default)
        {
            if (sozlesme == null) throw new ArgumentNullException(nameof(sozlesme));

            // Basic validation: must have either KiraciId or TasinmazId or a contract number
            var hasParty = sozlesme.KiraciId.HasValue || sozlesme.TasinmazId.HasValue || !string.IsNullOrWhiteSpace(sozlesme.SozlesmeNo);
            if (!hasParty)
            {
                _logService?.LogWarning("KiraSozlesmeService.AddAsync: KiraciId, TasinmazId veya SozlesmeNo gerekli.");
                return false;
            }

            try
            {
                await _unitOfWork.KiraSozlesme.AddAsync(sozlesme, cancellationToken);
                await _unitOfWork.KiraSozlesme.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("Kira sözleşmesi eklerken hata.", ex);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(KiraSozlesme sozlesme, CancellationToken cancellationToken = default)
        {
            if (sozlesme == null) throw new ArgumentNullException(nameof(sozlesme));

            var hasParty = sozlesme.KiraciId.HasValue || sozlesme.TasinmazId.HasValue || !string.IsNullOrWhiteSpace(sozlesme.SozlesmeNo);
            if (!hasParty)
            {
                _logService?.LogWarning("KiraSozlesmeService.UpdateAsync: KiraciId, TasinmazId veya SozlesmeNo gerekli.");
                return false;
            }

            try
            {
                await _unitOfWork.KiraSozlesme.UpdateAsync(sozlesme, null, cancellationToken);
                await _unitOfWork.KiraSozlesme.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("Kira sözleşmesi güncellerken hata.", ex);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int sozlesmeId, CancellationToken cancellationToken = default)
        {
            var entity = await _unitOfWork.KiraSozlesme.GetByIdAsync(sozlesmeId, cancellationToken);
            if (entity == null) return false;

            _unitOfWork.KiraSozlesme.Remove(entity);
            await _unitOfWork.KiraSozlesme.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> AnyAsync(Expression<Func<KiraSozlesme, bool>> predicate, CancellationToken cancellationToken = default)
            => await _unitOfWork.KiraSozlesme.AnyAsync(predicate, cancellationToken);
    }
}
