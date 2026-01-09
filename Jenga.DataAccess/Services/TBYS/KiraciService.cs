using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.TBYS;
using Jenga.Utility.Logging;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.TBYS
{
    public class KiraciService : IKiraciService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogService _logService;

        public KiraciService(IUnitOfWork unitOfWork, ILogService logService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logService = logService;
        }

        public async Task<List<Kiraci>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _unitOfWork.Kiraci.GetAllAsync(cancellationToken);

        public async Task<Kiraci?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => await _unitOfWork.Kiraci.GetByIdAsync(id, cancellationToken);

        public async Task<bool> AddAsync(Kiraci kiraci, CancellationToken cancellationToken = default)
        {
            if (kiraci == null) throw new ArgumentNullException(nameof(kiraci));

            // At least one name should be provided
            var name = (kiraci.Adi ?? string.Empty).Trim();
            var surname = (kiraci.Soyadi ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(surname))
            {
                _logService?.LogWarning("KiraciService.AddAsync: Adi veya Soyadi boş olamaz.");
                return false;
            }

            try
            {
                await _unitOfWork.Kiraci.AddAsync(kiraci, cancellationToken);
                await _unitOfWork.Kiraci.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("Kiracı eklerken hata.", ex);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(Kiraci kiraci, CancellationToken cancellationToken = default)
        {
            if (kiraci == null) throw new ArgumentNullException(nameof(kiraci));

            var name = (kiraci.Adi ?? string.Empty).Trim();
            var surname = (kiraci.Soyadi ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(surname))
            {
                _logService?.LogWarning("KiraciService.UpdateAsync: Adi veya Soyadi boş olamaz.");
                return false;
            }

            try
            {
                await _unitOfWork.Kiraci.UpdateAsync(kiraci, null, cancellationToken);
                await _unitOfWork.Kiraci.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("Kiracı güncellerken hata.", ex);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int kiraciId, CancellationToken cancellationToken = default)
        {
            var entity = await _unitOfWork.Kiraci.GetByIdAsync(kiraciId, cancellationToken);
            if (entity == null) return false;

            _unitOfWork.Kiraci.Remove(entity);
            await _unitOfWork.Kiraci.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> AnyAsync(Expression<Func<Kiraci, bool>> predicate, CancellationToken cancellationToken = default)
            => await _unitOfWork.Kiraci.AnyAsync(predicate, cancellationToken);
    }
}
