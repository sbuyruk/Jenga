using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.TBYS;
using Jenga.Utility.Logging;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.TBYS
{
    public class BagisciTalepleriService : IBagisciTalepleriService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogService _logService;

        public BagisciTalepleriService(IUnitOfWork unitOfWork, ILogService logService)
        {
            _unitOfWork = unitOfWork;
            _logService = logService;
        }

        public async Task<List<BagisciTalepleri>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _unitOfWork.BagisciTalepleri.GetAllAsync(cancellationToken);

        public async Task<BagisciTalepleri?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => await _unitOfWork.BagisciTalepleri.GetByIdAsync(id, cancellationToken);

        public async Task<List<BagisciTalepleri>> GetByBagisciIdAsync(long bagisciId, CancellationToken cancellationToken = default)
            => await _unitOfWork.BagisciTalepleri.GetByBagisciIdAsync(bagisciId, cancellationToken);

        public async Task<bool> AddAsync(BagisciTalepleri entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            try
            {
                await _unitOfWork.BagisciTalepleri.AddAsync(entity, cancellationToken);
                await _unitOfWork.BagisciTalepleri.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("BagisciTalepleriService.AddAsync hata.", ex);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(BagisciTalepleri entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            try
            {
                await _unitOfWork.BagisciTalepleri.UpdateAsync(entity, null, cancellationToken);
                await _unitOfWork.BagisciTalepleri.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("BagisciTalepleriService.UpdateAsync hata.", ex);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _unitOfWork.BagisciTalepleri.GetByIdAsync(id, cancellationToken);
            if (entity == null) return false;
            try
            {
                _unitOfWork.BagisciTalepleri.Remove(entity);
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("BagisciTalepleriService.DeleteAsync hata.", ex);
                throw;
            }
        }

        public async Task<bool> AnyAsync(Expression<Func<BagisciTalepleri, bool>> predicate, CancellationToken cancellationToken = default)
            => await _unitOfWork.BagisciTalepleri.AnyAsync(predicate, cancellationToken);
    }
}
