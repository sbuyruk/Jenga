using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.Inventory;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.Inventory
{
    public class MaterialAssetLogService : IMaterialAssetLogService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MaterialAssetLogService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<MaterialAssetLog>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _unitOfWork.MaterialAssetLog.GetAllAsync(cancellationToken);

        public async Task<MaterialAssetLog?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => await _unitOfWork.MaterialAssetLog.GetByIdAsync(id, cancellationToken);

        public async Task<List<MaterialAssetLog>> GetByAssetIdAsync(int materialAssetId, CancellationToken cancellationToken = default)
            => await _unitOfWork.MaterialAssetLog.GetByAssetIdAsync(materialAssetId, cancellationToken);

        public async Task<bool> AddAsync(MaterialAssetLog log, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.MaterialAssetLog.AddAsync(log, cancellationToken);
            await _unitOfWork.MaterialAssetLog.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAsync(MaterialAssetLog log, CancellationToken cancellationToken = default)
        {
            _unitOfWork.MaterialAssetLog.Remove(log);
            await _unitOfWork.MaterialAssetLog.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> AnyAsync(Expression<Func<MaterialAssetLog, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var items = await _unitOfWork.MaterialAssetLog.GetAllAsync(cancellationToken);
            return items.Any(predicate.Compile());
        }
    }
}
