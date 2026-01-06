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

        public async Task<MaterialInventory?> GetByMaterialLocationAsync(int materialId, int? locationId, int? personelId, int? brandId = null, int? modelId = null, CancellationToken cancellationToken = default)
            => await _unitOfWork.MaterialInventory.GetByMaterialLocationAsync(materialId, locationId, personelId, brandId, modelId, cancellationToken);

        public async Task AddOrUpdateInventoryAsync(
            int materialId,
            int? locationId,
            int? personelId,
            int quantity,
            string aciklama,
            string? modifiedBy,
            int? brandId = null,
            int? modelId = null,
            CancellationToken cancellationToken = default)
        {
            // Attempt to find existing inventory row for material+location+person+brand+model
            var existing = await _unitOfWork.MaterialInventory
                .GetByMaterialLocationAsync(materialId, locationId, personelId, brandId, modelId, cancellationToken);

            if (existing != null)
            {
                // compute new quantity
                var newQty = existing.Quantity + quantity;

                // Prevent negative resulting stock
                if (newQty < 0)
                {
                    throw new InvalidOperationException($"Yetersiz stok: mevcut {existing.Quantity}, yapılmak istenen değişiklik {quantity}. İşlem yapılmadı.");
                }

                existing.Quantity = newQty;
                existing.Aciklama = aciklama;
                await _unitOfWork.MaterialInventory.UpdateAsync(existing, modifiedBy);
            }
            else
            {
                // If adding a new row with negative quantity -> not allowed
                if (quantity < 0)
                {
                    throw new InvalidOperationException("Yeni bir stok kaydı eklendiğinde negatif miktar belirtilemez.");
                }

                var inventory = new MaterialInventory
                {
                    MaterialId = materialId,
                    LocationId = locationId,
                    PersonelId = personelId,
                    Quantity = quantity,
                    Aciklama = aciklama,
                    Olusturan = modifiedBy,
                    OlusturmaTarihi = DateTime.Now,
                    BrandId = brandId,
                    ModelId = modelId
                };
                await _unitOfWork.MaterialInventory.AddAsync(inventory, cancellationToken);
            }
            await _unitOfWork.MaterialInventory.SaveChangesAsync(cancellationToken);
        }

        // other methods unchanged...
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
            _unitOfWork.MaterialInventory.Remove(inventory);
            await _unitOfWork.MaterialInventory.SaveChangesAsync(cancellationToken);
        }
        public Task<bool> AnyAsync(Expression<Func<MaterialInventory, bool>> predicate)
        {
            return _unitOfWork.MaterialInventory.AnyAsync(predicate);
        }
    }
}