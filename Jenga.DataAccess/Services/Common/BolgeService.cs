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
    public class BolgeService : IBolgeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogService _logService;
        private List<Bolge>? _cache;

        public BolgeService(IUnitOfWork unitOfWork, ILogService logService)
        {
            _unitOfWork = unitOfWork;
            _logService = logService;
        }

        public async Task<List<Bolge>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            if (_cache == null)
            {
                _cache = await _unitOfWork.Bolge.GetAllAsync(cancellationToken);
            }
            return _cache;
        }

        public async Task<Bolge?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => await _unitOfWork.Bolge.GetByIdAsync(id, cancellationToken);

        public async Task<Bolge?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            try
            {
                return await _unitOfWork.Bolge.GetByNameAsync(name.Trim(), cancellationToken);
            }
            catch (Exception ex)
            {
                _logService?.LogError($"BolgeService.GetByNameAsync hata (name:{name})", ex);
                throw;
            }
        }

        public async Task<bool> AddAsync(Bolge bolge, CancellationToken cancellationToken = default)
        {
            if (bolge == null) throw new ArgumentNullException(nameof(bolge));
            try
            {
                await _unitOfWork.Bolge.AddAsync(bolge, cancellationToken);
                await _unitOfWork.Bolge.SaveChangesAsync(cancellationToken);
                _cache = null;
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("BolgeService.AddAsync hata.", ex);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(Bolge bolge, CancellationToken cancellationToken = default)
        {
            if (bolge == null) throw new ArgumentNullException(nameof(bolge));
            try
            {
                await _unitOfWork.Bolge.UpdateAsync(bolge, null, cancellationToken);
                await _unitOfWork.Bolge.SaveChangesAsync(cancellationToken);
                _cache = null;
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("BolgeService.UpdateAsync hata.", ex);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int bolgeId, CancellationToken cancellationToken = default)
        {
            var entity = await _unitOfWork.Bolge.GetByIdAsync(bolgeId, cancellationToken);
            if (entity != null)
            {
                _unitOfWork.Bolge.Remove(entity);
                await _unitOfWork.Bolge.SaveChangesAsync(cancellationToken);
                _cache = null;
                return true;
            }
            return false;
        }

        public async Task<bool> AnyAsync(Expression<Func<Bolge, bool>> predicate, CancellationToken cancellationToken = default)
            => await _unitOfWork.Bolge.AnyAsync(predicate, cancellationToken);
    }
}
