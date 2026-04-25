using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.TBYS;
using Jenga.Utility.Logging;

namespace Jenga.DataAccess.Services.TBYS
{
    public class TasinmazTaahhutService : ITasinmazTaahhutService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogService _logService;

        public TasinmazTaahhutService(IUnitOfWork unitOfWork, ILogService logService)
        {
            _unitOfWork = unitOfWork;
            _logService = logService;
        }

        public async Task<List<TasinmazTaahhut>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _unitOfWork.TasinmazTaahhut.GetAllAsync(cancellationToken);

        public async Task<TasinmazTaahhut?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => await _unitOfWork.TasinmazTaahhut.GetByIdAsync(id, cancellationToken);

        public async Task<List<TasinmazTaahhut>> GetByTasinmazIdAsync(int tasinmazId, CancellationToken cancellationToken = default)
            => await _unitOfWork.TasinmazTaahhut.GetByTasinmazIdAsync(tasinmazId, cancellationToken);

        public async Task<List<TasinmazTaahhut>> GetByBagisciIdAsync(int bagisciId, CancellationToken cancellationToken = default)
            => await _unitOfWork.TasinmazTaahhut.GetByBagisciIdAsync(bagisciId, cancellationToken);

        public async Task<bool> AddAsync(TasinmazTaahhut entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            try
            {
                await _unitOfWork.TasinmazTaahhut.AddAsync(entity, cancellationToken);
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
                await _unitOfWork.TasinmazTaahhut.UpdateAsync(entity, cancellationToken);
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
            var entity = await _unitOfWork.TasinmazTaahhut.GetByIdAsync(id, cancellationToken);
            if (entity == null) return false;
            try
            {
                _unitOfWork.TasinmazTaahhut.Remove(entity);
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
