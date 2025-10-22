using Jenga.DataAccess.Repositories.IRepository;
using Jenga.DataAccess.Repositories.IRepository.Inventory;
using Jenga.Models.Inventory;
using Jenga.Utility.Logging;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.Inventory
{
    public class MaterialService : IMaterialService
    {
        private readonly IUnitOfWork _unitOfWork;
        // Material list cache
        private List<Material>? _materialsCache;

        public MaterialService(IUnitOfWork unitOfWork, ILogService logService)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<Material>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _unitOfWork.Material.GetAllAsync(cancellationToken);

        public async Task<Material?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => await _unitOfWork.Material.GetByIdAsync(id, cancellationToken);

        public async Task<Material?> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default)
            => await _unitOfWork.Material.GetByIdWithRelationsAsync(id, cancellationToken);

        public async Task<bool> AddAsync(Material material, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.Material.AddAsync(material, cancellationToken);
            await _unitOfWork.Material.SaveChangesAsync(cancellationToken);
            // Invalidate cache if exists
            _materialsCache = null;
            return true;
        }

        public async Task<bool> UpdateAsync(Material material, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.Material.UpdateAsync(material);
            await _unitOfWork.Material.SaveChangesAsync(cancellationToken);
            _materialsCache = null;
            return true;
        }

        public async Task<bool> DeleteAsync(Material material, CancellationToken cancellationToken = default)
        {
            _unitOfWork.Material.Remove(material);
            await _unitOfWork.Material.SaveChangesAsync(cancellationToken);
            _materialsCache = null;
            return true;
        }

        public async Task<bool> AnyAsync(Expression<Func<Material, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var materials = await _unitOfWork.Material.GetAllAsync(cancellationToken);
            return materials.Any(predicate.Compile());
        }

        // Yardımcı Metotlar
        public async Task<string> GetMaterialNameAsync(int materialId, CancellationToken cancellationToken = default)
        {
            if (_materialsCache == null)
                _materialsCache = await GetAllAsync(cancellationToken);
            var material = _materialsCache.FirstOrDefault(x => x.Id == materialId);
            return material?.MaterialName ?? "";
        }

        public async Task<int> GetUnitIdAsync(int materialId, CancellationToken cancellationToken = default)
        {
            if (_materialsCache == null)
                _materialsCache = await GetAllAsync(cancellationToken);
            var material = _materialsCache.FirstOrDefault(x => x.Id == materialId);
            return material?.MaterialUnitId ?? 0;
        }
    }
}