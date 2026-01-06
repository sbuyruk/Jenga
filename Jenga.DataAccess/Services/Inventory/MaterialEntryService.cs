using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.Inventory;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.Inventory
{
    public class MaterialEntryService : IMaterialEntryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMaterialInventoryService _materialInventoryService;
        private readonly IMaterialMovementService _materialMovementService;

        public MaterialEntryService(
            IUnitOfWork unitOfWork,
            IMaterialInventoryService materialInventoryService,
            IMaterialMovementService materialMovementService)
        {
            _unitOfWork = unitOfWork;
            _materialInventoryService = materialInventoryService;
            _materialMovementService = materialMovementService;
        }

        public async Task<List<MaterialEntry>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _unitOfWork.MaterialEntry.GetAllAsync(cancellationToken);

        public async Task<MaterialEntry?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => await _unitOfWork.MaterialEntry.GetByIdAsync(id, cancellationToken);

        public async Task<bool> AddAsync(MaterialEntry entry, string? modifiedBy, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.MaterialEntry.AddAsync(entry, cancellationToken);
            await _unitOfWork.MaterialEntry.SaveChangesAsync(cancellationToken);

            // Normalize incoming zero IDs to null
            int? actualLocationId = entry.LocationId != 0 ? entry.LocationId : null;
            int? actualPersonnelId = (entry.PersonelId.HasValue && entry.PersonelId.Value != 0) ? entry.PersonelId : null;
            int? actualBrandId = (entry.BrandId.HasValue && entry.BrandId.Value != 0) ? entry.BrandId : null;
            int? actualModelId = (entry.ModelId.HasValue && entry.ModelId.Value != 0) ? entry.ModelId : null;

            // Update inventory - pass brand/model from entry if provided
            await _materialInventoryService.AddOrUpdateInventoryAsync(
                entry.MaterialId,
                actualLocationId,
                actualPersonnelId,
                entry.Quantity,
                "Malzeme girişi sonrası stok güncellemesi",
                modifiedBy,
                actualBrandId,
                actualModelId,
                cancellationToken
            );

            await _materialMovementService.AddMovementForEntryAsync(
                entry, "Giriş", "MaterialEntry eklendi", modifiedBy, cancellationToken
            );

            return true;
        }

        public async Task<bool> UpdateMaterialEntryAndInventoryAsync(MaterialEntry entry, string? currentUserName, CancellationToken cancellationToken = default)
        {
            var oldEntry = await GetByIdAsync(entry.Id, cancellationToken);
            if (oldEntry == null) throw new Exception("Eski kayıt bulunamadı.");

            bool quantityChanged = entry.Quantity != oldEntry.Quantity;
            bool materialChanged = entry.MaterialId != oldEntry.MaterialId;
            bool locationChanged = entry.LocationId != oldEntry.LocationId;
            bool unitChanged = entry.MaterialUnitId != oldEntry.MaterialUnitId;
            bool personnelChanged = entry.PersonelId != oldEntry.PersonelId;
            bool brandChanged = entry.BrandId != oldEntry.BrandId;
            bool modelChanged = entry.ModelId != oldEntry.ModelId;

            currentUserName ??= Environment.UserName;

            // ID normalization
            int? newLocation = entry.LocationId != 0 ? entry.LocationId : null;
            int? newPersonnel = (entry.PersonelId.HasValue && entry.PersonelId.Value != 0) ? entry.PersonelId : null;
            int? newBrand = (entry.BrandId.HasValue && entry.BrandId.Value != 0) ? entry.BrandId : null;
            int? newModel = (entry.ModelId.HasValue && entry.ModelId.Value != 0) ? entry.ModelId : null;

            int? oldLocation = oldEntry.LocationId != 0 ? oldEntry.LocationId : null;
            int? oldPersonnel = (oldEntry.PersonelId.HasValue && oldEntry.PersonelId.Value != 0) ? oldEntry.PersonelId : null;
            int? oldBrand = (oldEntry.BrandId.HasValue && oldEntry.BrandId.Value != 0) ? oldEntry.BrandId : null;
            int? oldModel = (oldEntry.ModelId.HasValue && oldEntry.ModelId.Value != 0) ? oldEntry.ModelId : null;

            if (quantityChanged && !materialChanged && !locationChanged && !unitChanged && !personnelChanged && !brandChanged && !modelChanged)
            {
                int delta = entry.Quantity - oldEntry.Quantity;
                await _materialInventoryService.AddOrUpdateInventoryAsync(
                    entry.MaterialId,
                    newLocation,
                    newPersonnel,
                    delta,
                    "Kayıt güncellemesi (miktar değişikliği)",
                    currentUserName,
                    newBrand,
                    newModel,
                    cancellationToken);
            }
            else if (materialChanged || locationChanged || unitChanged || personnelChanged || brandChanged || modelChanged)
            {
                // Revert old inventory (subtract old quantity) using old brand/model
                await _materialInventoryService.AddOrUpdateInventoryAsync(
                    oldEntry.MaterialId,
                    oldLocation,
                    oldPersonnel,
                    -oldEntry.Quantity,
                    "Kayıt güncellemesi (eski stoktan düş)",
                    currentUserName,
                    oldBrand,
                    oldModel,
                    cancellationToken);

                // Add new inventory using new brand/model
                await _materialInventoryService.AddOrUpdateInventoryAsync(
                    entry.MaterialId,
                    newLocation,
                    newPersonnel,
                    entry.Quantity,
                    "Kayıt güncellemesi (yeni stoğa ekle)",
                    currentUserName,
                    newBrand,
                    newModel,
                    cancellationToken);
            }

            await UpdateAsync(entry, cancellationToken);

            string movementType = (quantityChanged && !materialChanged && !locationChanged && !unitChanged && !personnelChanged && !brandChanged && !modelChanged) ? "Düzeltme" : "Düzenleme";
            await _materialMovementService.AddMovementForEntryAsync(
                entry, movementType, "MaterialEntry güncellendi", currentUserName, cancellationToken
            );

            return true;
        }

        public async Task<bool> UpdateAsync(MaterialEntry entry, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.MaterialEntry.UpdateAsync(entry);
            await _unitOfWork.MaterialEntry.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAsync(MaterialEntry entry, CancellationToken cancellationToken = default)
        {
            _unitOfWork.MaterialEntry.Remove(entry);
            await _unitOfWork.MaterialEntry.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteMaterialEntryAndUpdateInventoryAsync(MaterialEntry entryToDelete, string? currentUserName, CancellationToken cancellationToken = default)
        {
            currentUserName ??= Environment.UserName;
            if (entryToDelete == null) return false;

            int? location = entryToDelete.LocationId != 0 ? entryToDelete.LocationId : null;
            int? personnel = (entryToDelete.PersonelId.HasValue && entryToDelete.PersonelId.Value != 0) ? entryToDelete.PersonelId : null;
            int? brand = (entryToDelete.BrandId.HasValue && entryToDelete.BrandId.Value != 0) ? entryToDelete.BrandId : null;
            int? model = (entryToDelete.ModelId.HasValue && entryToDelete.ModelId.Value != 0) ? entryToDelete.ModelId : null;

            // 1. Subtract from inventory
            await _materialInventoryService.AddOrUpdateInventoryAsync(
                entryToDelete.MaterialId,
                location,
                personnel,
                -entryToDelete.Quantity,
                "MaterialEntry silindi, stoktan çıkarıldı",
                currentUserName,
                brand,
                model,
                cancellationToken
            );

            // 2. If stock <= 0 remove inventory record (cleanup)
            var inventoryRecord = await _materialInventoryService.GetByMaterialLocationAsync(
                entryToDelete.MaterialId,
                location,
                personnel,
                brand,
                model,
                cancellationToken
            );
            if (inventoryRecord != null && inventoryRecord.Quantity <= 0)
            {
                await _materialInventoryService.DeleteAsync(inventoryRecord, cancellationToken);
            }

            // 3. Delete the entry
            await DeleteAsync(entryToDelete, cancellationToken);

            await _materialMovementService.AddMovementForEntryAsync(
                entryToDelete, "Silme", "MaterialEntry silindi", currentUserName, cancellationToken
            );

            return true;
        }

        public Task<bool> AnyAsync(Expression<Func<MaterialEntry, bool>> predicate)
        {
            return _unitOfWork.MaterialEntry.AnyAsync(predicate);
        }
    }
}