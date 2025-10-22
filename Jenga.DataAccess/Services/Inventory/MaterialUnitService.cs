using Jenga.DataAccess.Repositories.IRepository;
using Jenga.DataAccess.Repositories.IRepository.Inventory;
using Jenga.Models.Inventory;

namespace Jenga.DataAccess.Services.Inventory
{
    public class MaterialUnitService : IMaterialUnitService
    {
        private readonly IUnitOfWork _unitOfWork;
        // MaterialUnit cache
        private List<MaterialUnit>? _unitsCache;

        public MaterialUnitService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<MaterialUnit>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _unitOfWork.MaterialUnit.GetAllAsync(cancellationToken);

        public async Task<MaterialUnit?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => await _unitOfWork.MaterialUnit.GetByIdAsync(id, cancellationToken);

        public async Task<bool> AddAsync(MaterialUnit unit, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.MaterialUnit.AddAsync(unit, cancellationToken);
            await _unitOfWork.MaterialUnit.SaveChangesAsync(cancellationToken);
            _unitsCache = null;
            return true;
        }

        public async Task<bool> UpdateAsync(MaterialUnit unit, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.MaterialUnit.UpdateAsync(unit);
            await _unitOfWork.MaterialUnit.SaveChangesAsync(cancellationToken);
            _unitsCache = null;
            return true;
        }

        public async Task<bool> DeleteAsync(MaterialUnit unit, CancellationToken cancellationToken = default)
        {
            _unitOfWork.MaterialUnit.Remove(unit);
            await _unitOfWork.MaterialUnit.SaveChangesAsync(cancellationToken);
            _unitsCache = null;
            return true;
        }

        // Yardımcı Metot
        public async Task<string> GetUnitSymbolAsync(int unitId, CancellationToken cancellationToken = default)
        {
            if (_unitsCache == null)
                _unitsCache = await GetAllAsync(cancellationToken);
            var unit = _unitsCache.FirstOrDefault(x => x.Id == unitId);
            return unit?.Symbol ?? "";
        }
    }
}