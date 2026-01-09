using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.TBYS;
using Jenga.Utility.Logging;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.TBYS
{
    public class OdemePlaniService : IOdemePlaniService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogService _logService;

        public OdemePlaniService(IUnitOfWork unitOfWork, ILogService logService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logService = logService;
        }

        public async Task<List<OdemePlani>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _unitOfWork.OdemePlani.GetAllAsync(cancellationToken);

        public async Task<OdemePlani?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => await _unitOfWork.OdemePlani.GetByIdAsync(id, cancellationToken);

        public async Task<bool> AddAsync(OdemePlani odemePlani, CancellationToken cancellationToken = default)
        {
            if (odemePlani == null) throw new ArgumentNullException(nameof(odemePlani));

            // Basit doğrulama: bir sözleşme id'si olmalı
            if (!odemePlani.SozlesmeId.HasValue)
            {
                _logService?.LogWarning("OdemePlaniService.AddAsync: SozlesmeId gerekli.");
                return false;
            }

            try
            {
                await _unitOfWork.OdemePlani.AddAsync(odemePlani, cancellationToken);
                await _unitOfWork.OdemePlani.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("Ödeme planı eklerken hata.", ex);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(OdemePlani odemePlani, CancellationToken cancellationToken = default)
        {
            if (odemePlani == null) throw new ArgumentNullException(nameof(odemePlani));

            if (!odemePlani.SozlesmeId.HasValue)
            {
                _logService?.LogWarning("OdemePlaniService.UpdateAsync: SozlesmeId gerekli.");
                return false;
            }

            try
            {
                await _unitOfWork.OdemePlani.UpdateAsync(odemePlani, null, cancellationToken);
                await _unitOfWork.OdemePlani.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("Ödeme planı güncellerken hata.", ex);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _unitOfWork.OdemePlani.GetByIdAsync(id, cancellationToken);
            if (entity == null) return false;

            _unitOfWork.OdemePlani.Remove(entity);
            await _unitOfWork.OdemePlani.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> AnyAsync(Expression<Func<OdemePlani, bool>> predicate, CancellationToken cancellationToken = default)
            => await _unitOfWork.OdemePlani.AnyAsync(predicate, cancellationToken);
    }
}
