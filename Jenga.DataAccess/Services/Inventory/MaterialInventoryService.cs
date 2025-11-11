using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.Inventory;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.Inventory
{
    public class MaterialInventoryService : IMaterialInventoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MaterialInventoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<MaterialInventory>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _unitOfWork.MaterialInventory.GetAllAsync(cancellationToken);

        public async Task<MaterialInventory?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => await _unitOfWork.MaterialInventory.GetByIdAsync(id, cancellationToken);

        public async Task<MaterialInventory?> GetByMaterialLocationAsync(int materialId, int locationId, int materialUnitId, CancellationToken cancellationToken = default)
            => await _unitOfWork.MaterialInventory.GetByMaterialLocationAsync(materialId, locationId, materialUnitId, cancellationToken);

        public async Task AddOrUpdateInventoryAsync(
            int materialId,
            int locationId,
            int materialUnitId,
            int quantity,
            string aciklama,
            string? modifiedBy,
            CancellationToken cancellationToken = default)
        {
            var existing = await _unitOfWork.MaterialInventory
                .GetByMaterialLocationAsync(materialId, locationId, materialUnitId, cancellationToken);

            if (existing != null)
            {
                existing.Quantity += quantity;
                existing.Aciklama = aciklama;
                await _unitOfWork.MaterialInventory.UpdateAsync(existing, modifiedBy);
            }
            else
            {
                var inventory = new MaterialInventory
                {
                    MaterialId = materialId,
                    LocationId = locationId,
                    MaterialUnitId = materialUnitId,
                    Quantity = quantity,
                    Aciklama = aciklama,
                    Olusturan = modifiedBy,
                    OlusturmaTarihi = DateTime.Now
                };
                await _unitOfWork.MaterialInventory.AddAsync(inventory, cancellationToken);
            }
            await _unitOfWork.MaterialInventory.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateInventoryAsync(MaterialInventory inventory, string? modifiedBy, CancellationToken cancellationToken = default)
        {
            modifiedBy ??= Environment.UserName;
            await _unitOfWork.MaterialInventory.UpdateAsync(inventory, modifiedBy);
            await _unitOfWork.MaterialInventory.SaveChangesAsync(cancellationToken);
        }

        public async Task AddAsync(MaterialInventory inventory, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.MaterialInventory.AddAsync(inventory, cancellationToken);
            await _unitOfWork.MaterialInventory.SaveChangesAsync(cancellationToken);
        }
        public async Task DeleteAsync(MaterialInventory inventory, CancellationToken cancellationToken = default)
        {
            // _unitOfWork.MaterialInventory.Remove ile kaydı sil
            _unitOfWork.MaterialInventory.Remove(inventory);
            await _unitOfWork.MaterialInventory.SaveChangesAsync(cancellationToken);
        }
        public Task<bool> AnyAsync(Expression<Func<MaterialInventory, bool>> predicate)
        {
            return _unitOfWork.MaterialInventory.AnyAsync(predicate);
        }
    }

}