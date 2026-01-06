using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.Common;
using Jenga.Utility.Logging;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Jenga.DataAccess.Services.Common
{
    public class IlService : IIlService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogService _logService;
        private List<Il>? _cache;

        public IlService(IUnitOfWork unitOfWork, ILogService logService)
        {
            _unitOfWork = unitOfWork;
            _logService = logService;
        }

        public async Task<List<Il>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            // simple passthrough to repository; cache for short-term use
            if (_cache == null)
            {
                _cache = await _unitOfWork.Il.GetAllAsync(cancellationToken);
            }
            return _cache;
        }

        public async Task<Il?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => await _unitOfWork.Il.GetByIdAsync(id, cancellationToken);

        public async Task<bool> AddAsync(Il il, CancellationToken cancellationToken = default)
        {
            if (il == null) throw new ArgumentNullException(nameof(il));
            try
            {
                await _unitOfWork.Il.AddAsync(il, cancellationToken);
                await _unitOfWork.Il.SaveChangesAsync(cancellationToken);
                _cache = null;
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("IlService.AddAsync hata.", ex);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(Il il, CancellationToken cancellationToken = default)
        {
            if (il == null) throw new ArgumentNullException(nameof(il));
            try
            {
                await _unitOfWork.Il.UpdateAsync(il, null, cancellationToken);
                await _unitOfWork.Il.SaveChangesAsync(cancellationToken);
                _cache = null;
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("IlService.UpdateAsync hata.", ex);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int ilId, CancellationToken cancellationToken = default)
        {
            var entity = await _unitOfWork.Il.GetByIdAsync(ilId, cancellationToken);
            if (entity != null)
            {
                _unitOfWork.Il.Remove(entity);
                await _unitOfWork.Il.SaveChangesAsync(cancellationToken);
                _cache = null;
                return true;
            }
            return false;
        }

        public async Task<bool> AnyAsync(Expression<Func<Il, bool>> predicate, CancellationToken cancellationToken = default)
            => await _unitOfWork.Il.AnyAsync(predicate, cancellationToken);
    }
}
