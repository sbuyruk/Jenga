using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.TBYS;
using Jenga.Utility.Logging;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.TBYS
{
    public class BagisciYakinlariService : IBagisciYakinlariService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogService _logService;

        public BagisciYakinlariService(IUnitOfWork unitOfWork, ILogService logService)
        {
            _unitOfWork = unitOfWork;
            _logService = logService;
        }

        public async Task<List<BagisciYakinlari>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _unitOfWork.BagisciYakinlari.GetAllAsync(cancellationToken);

        public async Task<BagisciYakinlari?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => await _unitOfWork.BagisciYakinlari.GetByIdAsync(id, cancellationToken);

        public async Task<List<BagisciYakinlari>> GetByBagisciIdAsync(long bagisciId, CancellationToken cancellationToken = default)
            => await _unitOfWork.BagisciYakinlari.GetByBagisciIdAsync(bagisciId, cancellationToken);

        public async Task<bool> AddAsync(BagisciYakinlari entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            try
            {
                await _unitOfWork.BagisciYakinlari.AddAsync(entity, cancellationToken);
                await _unitOfWork.BagisciYakinlari.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("BagisciYakinlariService.AddAsync hata.", ex);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(BagisciYakinlari entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            try
            {
                await _unitOfWork.BagisciYakinlari.UpdateAsync(entity, null, cancellationToken);
                await _unitOfWork.BagisciYakinlari.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("BagisciYakinlariService.UpdateAsync hata.", ex);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _unitOfWork.BagisciYakinlari.GetByIdAsync(id, cancellationToken);
            if (entity == null) return false;
            try
            {
                _unitOfWork.BagisciYakinlari.Remove(entity);
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("BagisciYakinlariService.DeleteAsync hata.", ex);
                throw;
            }
        }

        public async Task<bool> AnyAsync(Expression<Func<BagisciYakinlari, bool>> predicate, CancellationToken cancellationToken = default)
            => await _unitOfWork.BagisciYakinlari.AnyAsync(predicate, cancellationToken);
    }
}
