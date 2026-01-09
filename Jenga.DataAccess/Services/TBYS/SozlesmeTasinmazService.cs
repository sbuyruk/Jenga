using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.TBYS;
using Jenga.Utility.Logging;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.TBYS
{
    public class SozlesmeTasinmazService : ISozlesmeTasinmazService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogService _logService;

        public SozlesmeTasinmazService(IUnitOfWork unitOfWork, ILogService logService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logService = logService;
        }

        public async Task<List<SozlesmeTasinmaz>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _unitOfWork.SozlesmeTasinmaz.GetAllAsync(cancellationToken);

        public async Task<SozlesmeTasinmaz?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => await _unitOfWork.SozlesmeTasinmaz.GetByIdAsync(id, cancellationToken);

        public async Task<bool> AddAsync(SozlesmeTasinmaz entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            // Basic validation: at least SozlesmeId or TasinmazId should be provided
            if (!entity.SozlesmeId.HasValue && !entity.TasinmazId.HasValue)
            {
                _logService?.LogWarning("SozlesmeTasinmazService.AddAsync: SozlesmeId veya TasinmazId gerekli.");
                return false;
            }

            try
            {
                await _unitOfWork.SozlesmeTasinmaz.AddAsync(entity, cancellationToken);
                await _unitOfWork.SozlesmeTasinmaz.SaveChangesAsync(cancellationToken);
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
                await _unitOfWork.SozlesmeTasinmaz.UpdateAsync(entity, null, cancellationToken);
                await _unitOfWork.SozlesmeTasinmaz.SaveChangesAsync(cancellationToken);
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
            var entity = await _unitOfWork.SozlesmeTasinmaz.GetByIdAsync(id, cancellationToken);
            if (entity == null) return false;

            _unitOfWork.SozlesmeTasinmaz.Remove(entity);
            await _unitOfWork.SozlesmeTasinmaz.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> AnyAsync(Expression<Func<SozlesmeTasinmaz, bool>> predicate, CancellationToken cancellationToken = default)
            => await _unitOfWork.SozlesmeTasinmaz.AnyAsync(predicate, cancellationToken);
    }
}
