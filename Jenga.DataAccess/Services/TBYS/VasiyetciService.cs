using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.TBYS;
using Jenga.Utility.Logging;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.TBYS
{
    public class VasiyetciService : IVasiyetciService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogService _logService;

        public VasiyetciService(IUnitOfWork unitOfWork, ILogService logService)
        {
            _unitOfWork = unitOfWork;
            _logService = logService;
        }

        public async Task<List<Vasiyetci>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _unitOfWork.Vasiyetci.GetAllAsync(cancellationToken);

        public async Task<Vasiyetci?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => await _unitOfWork.Vasiyetci.GetByIdAsync(id, cancellationToken);

        public async Task<List<Vasiyetci>> GetByTCKimlikAsync(long tcKimlik, CancellationToken cancellationToken = default)
            => await _unitOfWork.Vasiyetci.GetByTCKimlikAsync(tcKimlik, cancellationToken);

        public async Task<bool> AddAsync(Vasiyetci entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            try
            {
                await _unitOfWork.Vasiyetci.AddAsync(entity, cancellationToken);
                await _unitOfWork.Vasiyetci.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("VasiyetciService.AddAsync hata.", ex);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(Vasiyetci entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            try
            {
                await _unitOfWork.Vasiyetci.UpdateAsync(entity, null, cancellationToken);
                await _unitOfWork.Vasiyetci.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("VasiyetciService.UpdateAsync hata.", ex);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _unitOfWork.Vasiyetci.GetByIdAsync(id, cancellationToken);
            if (entity == null) return false;
            try
            {
                _unitOfWork.Vasiyetci.Remove(entity);
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("VasiyetciService.DeleteAsync hata.", ex);
                throw;
            }
        }

        public async Task<bool> AnyAsync(Expression<Func<Vasiyetci, bool>> predicate, CancellationToken cancellationToken = default)
            => await _unitOfWork.Vasiyetci.AnyAsync(predicate, cancellationToken);
    }
}
