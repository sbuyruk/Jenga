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
    public class OdemeService : IOdemeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogService _logService;
        private List<Odeme>? _cache;

        public OdemeService(IUnitOfWork unitOfWork, ILogService logService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logService = logService;
        }

        public async Task<List<Odeme>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _unitOfWork.Odeme.GetAllAsync(cancellationToken);

        public async Task<Odeme?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => await _unitOfWork.Odeme.GetByIdAsync(id, cancellationToken);

        public async Task<Odeme?> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default)
            => await _unitOfWork.Odeme.GetByIdWithRelationsAsync(id, cancellationToken);

        public async Task<List<Odeme>> GetBySozlesmeIdAsync(int sozlesmeId, CancellationToken cancellationToken = default)
            => await _unitOfWork.Odeme.GetBySozlesmeIdAsync(sozlesmeId, cancellationToken);

        public async Task<List<Odeme>> GetByKiraciIdAsync(int kiraciId, CancellationToken cancellationToken = default)
            => await _unitOfWork.Odeme.GetByKiraciIdAsync(kiraciId, cancellationToken);

        public async Task<List<Odeme>> GetByOdemePlaniIdAsync(int odemePlaniId, CancellationToken cancellationToken = default)
            => await _unitOfWork.Odeme.GetByOdemePlaniIdAsync(odemePlaniId, cancellationToken);

        public async Task<bool> AddAsync(Odeme odeme, CancellationToken cancellationToken = default)
        {
            if (odeme == null) throw new ArgumentNullException(nameof(odeme));

            try
            {
                await _unitOfWork.Odeme.AddAsync(odeme, cancellationToken);
                await _unitOfWork.Odeme.SaveChangesAsync(cancellationToken);
                _cache = null;
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("OdemeService.AddAsync hata.", ex);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(Odeme odeme, CancellationToken cancellationToken = default)
        {
            if (odeme == null) throw new ArgumentNullException(nameof(odeme));

            try
            {
                await _unitOfWork.Odeme.UpdateAsync(odeme, null, cancellationToken);
                await _unitOfWork.Odeme.SaveChangesAsync(cancellationToken);
                _cache = null;
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("OdemeService.UpdateAsync hata.", ex);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int odemeId, CancellationToken cancellationToken = default)
        {
            var entity = await _unitOfWork.Odeme.GetByIdAsync(odemeId, cancellationToken);
            if (entity != null)
            {
                _unitOfWork.Odeme.Remove(entity);
                await _unitOfWork.Odeme.SaveChangesAsync(cancellationToken);
                _cache = null;
                return true;
            }
            return false;
        }

        public async Task<bool> AnyAsync(Expression<Func<Odeme, bool>> predicate, CancellationToken cancellationToken = default)
            => await _unitOfWork.Odeme.AnyAsync(predicate, cancellationToken);

        public async Task<(bool CanDelete, string? Reason)> CanDeleteAsync(int id)
        {
            var entity = await _unitOfWork.Odeme.GetByIdAsync(id);
            if (entity == null) return (false, "Kayıt bulunamadı.");
            // add domain-specific checks here if necessary
            return (true, null);
        }
    }
}
