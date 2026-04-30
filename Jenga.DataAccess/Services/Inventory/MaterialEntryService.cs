using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.Inventory;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.Inventory
{
    public class MaterialEntryService : IMaterialEntryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMaterialInventoryService _materialInventoryService;
        private readonly IMaterialMovementService _materialMovementService;
        private readonly IMaterialAssetService _materialAssetService;
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public MaterialEntryService(
            IUnitOfWork unitOfWork,
            IMaterialInventoryService materialInventoryService,
            IMaterialMovementService materialMovementService,
            IMaterialAssetService materialAssetService,
            IDbContextFactory<ApplicationDbContext> dbFactory)
        {
            _unitOfWork = unitOfWork;
            _materialInventoryService = materialInventoryService;
            _materialMovementService = materialMovementService;
            _materialAssetService = materialAssetService;
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        }

        public async Task<List<MaterialEntry>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _unitOfWork.MaterialEntry.GetAllAsync(cancellationToken);

        public async Task<MaterialEntry?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => await _unitOfWork.MaterialEntry.GetByIdAsync(id, cancellationToken);

        public async Task<bool> AddAsync(MaterialEntry entry, string? modifiedBy, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.MaterialEntry.AddAsync(entry, cancellationToken);
            await _unitOfWork.MaterialEntry.SaveChangesAsync(cancellationToken);

            int? actualLocationId = entry.LocationId != 0 ? entry.LocationId : null;
            int? actualPersonnelId = (entry.PersonelId.HasValue && entry.PersonelId.Value != 0) ? entry.PersonelId : null;
            int? actualBrandId = (entry.BrandId.HasValue && entry.BrandId.Value != 0) ? entry.BrandId : null;
            int? actualModelId = (entry.ModelId.HasValue && entry.ModelId.Value != 0) ? entry.ModelId : null;

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

            var material = await _unitOfWork.Material.GetByIdAsync(entry.MaterialId, cancellationToken);
            if (material != null && material.IsAsset)
            {
                for (int i = 0; i < entry.Quantity; i++)
                {
                    var asset = new MaterialAsset
                    {
                        MaterialId = entry.MaterialId,
                        BrandId = actualBrandId,
                        ModelId = actualModelId,
                        LocationId = actualLocationId,
                        PersonelId = actualPersonnelId,
                        PurchaseDate = entry.EntryDate,
                        Status = AssetStatus.Active,
                        Aciklama = $"MaterialEntry #{entry.Id} ile otomatik oluşturuldu",
                        Olusturan = modifiedBy,
                        OlusturmaTarihi = DateTime.Now
                    };
                    await _materialAssetService.AddAsync(asset, cancellationToken);
                }
            }

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
            if (entryToDelete == null) return false;
            currentUserName ??= Environment.UserName;

            int? location = entryToDelete.LocationId != 0 ? entryToDelete.LocationId : null;
            int? personnel = (entryToDelete.PersonelId.HasValue && entryToDelete.PersonelId.Value != 0) ? entryToDelete.PersonelId : null;
            int? brand = (entryToDelete.BrandId.HasValue && entryToDelete.BrandId.Value != 0) ? entryToDelete.BrandId : null;
            int? model = (entryToDelete.ModelId.HasValue && entryToDelete.ModelId.Value != 0) ? entryToDelete.ModelId : null;

            // Canary (Adım 1): tek context + tek transaction içinde
            //   1) Inventory: -Quantity uygula (yoksa negatif ekleme yasak; orijinal davranış)
            //   2) Inventory satırı <= 0 düştüyse sil
            //   3) MaterialEntry satırını sil
            //   4) MaterialMovement "Silme" logu ekle
            // Hata olursa using sonu rollback eder; "stok düştü ama entry kaldı" gibi tutarsızlıklar oluşmaz.
            try
            {
                await using var scope = await DbContextScope.CreateAsync(_dbFactory, cancellationToken);
                var db = scope.Context;

                // 1) Inventory satırını bul (5'li anahtar, NULL eşleşmeleri dahil).
                var inventory = await db.MaterialInventory_Table
                    .FirstOrDefaultAsync(mi =>
                        mi.MaterialId == entryToDelete.MaterialId &&
                        mi.LocationId == location &&
                        mi.PersonelId == personnel &&
                        mi.BrandId == brand &&
                        mi.ModelId == model,
                        cancellationToken);

                int delta = -entryToDelete.Quantity;
                if (inventory != null)
                {
                    var newQty = inventory.Quantity + delta;
                    if (newQty < 0)
                        throw new InvalidOperationException($"Yetersiz stok: mevcut {inventory.Quantity}, yapılmak istenen değişiklik {delta}. İşlem yapılmadı.");

                    inventory.Quantity = newQty;
                    inventory.Aciklama = "MaterialEntry silindi, stoktan çıkarıldı";
                    inventory.Degistiren = currentUserName;
                    inventory.DegistirmeTarihi = DateTime.Now;

                    // 2) Sıfır veya altına düştüyse satırı kaldır.
                    if (newQty <= 0)
                        db.MaterialInventory_Table.Remove(inventory);
                }
                else
                {
                    // Mevcut servis davranışı: yeni satır eklerken negatif miktar yasak.
                    if (delta < 0)
                        throw new InvalidOperationException("Yeni bir stok kaydı eklendiğinde negatif miktar belirtilemez.");
                }

                // 3) MaterialEntry satırını sil. Detached olabileceği için Attach + Remove ile state'i kesinleştiriyoruz.
                var entryEntity = await db.MaterialEntry_Table
                    .FirstOrDefaultAsync(e => e.Id == entryToDelete.Id, cancellationToken);
                if (entryEntity != null)
                    db.MaterialEntry_Table.Remove(entryEntity);

                // 4) MaterialMovement "Silme" logu (orijinal AddMovementForEntryAsync ile aynı alanlar).
                var movement = new MaterialMovement
                {
                    MaterialId = entryToDelete.MaterialId,
                    Quantity = entryToDelete.Quantity,
                    MaterialUnitId = entryToDelete.MaterialUnitId,
                    FromLocationId = null,
                    ToLocationId = entryToDelete.LocationId,
                    ToPersonId = entryToDelete.PersonelId,
                    BrandId = entryToDelete.BrandId,
                    ModelId = entryToDelete.ModelId,
                    MovementType = "Silme",
                    Operation = "Silme",
                    MovementDate = entryToDelete.EntryDate == default ? DateTime.Now : entryToDelete.EntryDate,
                    Aciklama = "MaterialEntry silindi",
                    Olusturan = currentUserName,
                    OlusturmaTarihi = DateTime.Now
                };
                await db.MaterialMovement_Table.AddAsync(movement, cancellationToken);

                await scope.CommitAsync(cancellationToken);
                return true;
            }
            catch
            {
                throw;
            }
        }

        public Task<bool> AnyAsync(Expression<Func<MaterialEntry, bool>> predicate)
        {
            return _unitOfWork.MaterialEntry.AnyAsync(predicate);
        }
    }
}