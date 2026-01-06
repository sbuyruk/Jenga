using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.TBYS;
using Jenga.Utility.Logging;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Jenga.DataAccess.Services.TBYS
{
    public class BagisService : IBagisService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogService _logService;
        private List<Bagis>? _cache;

        public BagisService(IUnitOfWork unitOfWork, ILogService logService)
        {
            _unitOfWork = unitOfWork;
            _logService = logService;
        }

        public async Task<List<Bagis>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _unitOfWork.Bagis.GetAllAsync(cancellationToken);

        public async Task<Bagis?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => await _unitOfWork.Bagis.GetByIdAsync(id, cancellationToken);

        public async Task<Bagis?> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default)
            => await _unitOfWork.Bagis.GetByIdWithRelationsAsync(id, cancellationToken);

        public async Task<List<Bagis>> GetByBagisciIdAsync(int bagisciId, CancellationToken cancellationToken = default)
            => await _unitOfWork.Bagis.GetByBagisciIdAsync(bagisciId, cancellationToken);

        public async Task<List<Bagis>> GetByTasinmazIdAsync(int tasinmazId, CancellationToken cancellationToken = default)
            => await _unitOfWork.Bagis.GetByTasinmazIdAsync(tasinmazId, cancellationToken);

        public async Task<bool> AddAsync(Bagis bagis, CancellationToken cancellationToken = default)
        {
            if (bagis == null) throw new ArgumentNullException(nameof(bagis));

            try
            {
                await _unitOfWork.Bagis.AddAsync(bagis, cancellationToken);
                await _unitOfWork.Bagis.SaveChangesAsync(cancellationToken);
                _cache = null;
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("BagisService.AddAsync hata.", ex);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(Bagis bagis, CancellationToken cancellationToken = default)
        {
            if (bagis == null) throw new ArgumentNullException(nameof(bagis));

            try
            {
                await _unitOfWork.Bagis.UpdateAsync(bagis, null, cancellationToken);
                await _unitOfWork.Bagis.SaveChangesAsync(cancellationToken);
                _cache = null;
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("BagisService.UpdateAsync hata.", ex);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int bagisId, CancellationToken cancellationToken = default)
        {
            var entity = await _unitOfWork.Bagis.GetByIdAsync(bagisId, cancellationToken);
            if (entity != null)
            {
                _unitOfWork.Bagis.Remove(entity);
                await _unitOfWork.Bagis.SaveChangesAsync(cancellationToken);
                _cache = null;
                return true;
            }
            return false;
        }

        public async Task<bool> AnyAsync(Expression<Func<Bagis, bool>> predicate, CancellationToken cancellationToken = default)
            => await _unitOfWork.Bagis.AnyAsync(predicate, cancellationToken);

        public async Task<(bool CanDelete, string? Reason)> CanDeleteAsync(int id)
        {
            // If there are other domain constraints preventing deletion, add checks here.
            // For now allow delete when the entity exists.
            var entity = await _unitOfWork.Bagis.GetByIdAsync(id);
            if (entity == null) return (false, "Kayıt bulunamadı.");
            return (true, null);
        }
    }
}