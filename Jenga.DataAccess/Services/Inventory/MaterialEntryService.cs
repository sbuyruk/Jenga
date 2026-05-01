using Jenga.DataAccess.Data;
using Jenga.Models.Inventory;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.Inventory
{
    public class MaterialEntryService : IMaterialEntryService
    {
        private const string Source = nameof(MaterialEntryService);

        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly IDbContextScopeFactory _scopeFactory;
        private readonly ILogService _logService;

        public MaterialEntryService(
            IDbContextFactory<ApplicationDbContext> dbFactory,
            IDbContextScopeFactory scopeFactory,
            ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _logService = logService;
        }

        public async Task<Result<List<MaterialEntry>>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.MaterialEntry_Table.AsNoTracking().ToListAsync(cancellationToken);
                return Result.Success(list);
            }
            catch (Exception ex)
            {
                _logService?.LogException(ex, $"{Source}.GetAllAsync");
                return Result.Failure<List<MaterialEntry>>(Error.Unexpected("Giriş kayıtları alınamadı.", ex, "MaterialEntry.GetAll.Failed"));
            }
        }

        public async Task<Result> AddAsync(MaterialEntry entry, string? modifiedBy = null, CancellationToken cancellationToken = default)
        {
            if (entry == null)
                return Result.Failure(Error.Validation("Giriş kaydı boş olamaz.", "MaterialEntry.Null"));
            try
            {

            int? actualLocationId = entry.LocationId != 0 ? entry.LocationId : null;
            int? actualPersonnelId = (entry.PersonelId.HasValue && entry.PersonelId.Value != 0) ? entry.PersonelId : null;
            int? actualBrandId = (entry.BrandId.HasValue && entry.BrandId.Value != 0) ? entry.BrandId : null;
            int? actualModelId = (entry.ModelId.HasValue && entry.ModelId.Value != 0) ? entry.ModelId : null;

            // Canary (Adım 2): tek context + tek transaction içinde
            //   1) MaterialEntry ekle ve Id'yi al
            //   2) Inventory: +Quantity uygula (yoksa yeni satır)
            //   3) MaterialMovement "Giriş" logu ekle
            //   4) Material.IsAsset ise N adet MaterialAsset üret
            // Hata olursa using sonu rollback eder; "entry kaldı ama inventory/movement/asset eksik" tutarsızlığı oluşmaz.
            await using var scope = await _scopeFactory.CreateAsync(cancellationToken);
            var db = scope.Context;

            // 1) MaterialEntry ekle. Id'ye sonraki adımlarda ihtiyacımız olduğu için ara SaveChanges yapıyoruz;
            //    aynı transaction içinde olduğundan rollback'i engellemez.
            await db.MaterialEntry_Table.AddAsync(entry, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);

            // 2) Inventory upsert (5'li anahtar, NULL eşleşmeleri dahil) — orijinal AddOrUpdateInventoryAsync ile aynı semantik.
            var inventory = await db.MaterialInventory_Table
                .FirstOrDefaultAsync(mi =>
                    mi.MaterialId == entry.MaterialId &&
                    mi.LocationId == actualLocationId &&
                    mi.PersonelId == actualPersonnelId &&
                    mi.BrandId == actualBrandId &&
                    mi.ModelId == actualModelId,
                    cancellationToken);

            int delta = entry.Quantity;
            const string invAciklama = "Malzeme girişi sonrası stok güncellemesi";
            if (inventory != null)
            {
                var newQty = inventory.Quantity + delta;
                if (newQty < 0)
                    throw new InvalidOperationException($"Yetersiz stok: mevcut {inventory.Quantity}, yapılmak istenen değişiklik {delta}. İşlem yapılmadı.");

                inventory.Quantity = newQty;
                inventory.Aciklama = invAciklama;
                inventory.Degistiren = modifiedBy;
                inventory.DegistirmeTarihi = DateTime.Now;
            }
            else
            {
                if (delta < 0)
                    throw new InvalidOperationException("Yeni bir stok kaydı eklendiğinde negatif miktar belirtilemez.");

                var newInventory = new MaterialInventory
                {
                    MaterialId = entry.MaterialId,
                    LocationId = actualLocationId,
                    PersonelId = actualPersonnelId,
                    BrandId = actualBrandId,
                    ModelId = actualModelId,
                    Quantity = delta,
                    Aciklama = invAciklama,
                    Olusturan = modifiedBy,
                    OlusturmaTarihi = DateTime.Now
                };
                await db.MaterialInventory_Table.AddAsync(newInventory, cancellationToken);
            }

            // 3) MaterialMovement "Giriş" logu (orijinal AddMovementForEntryAsync ile aynı alanlar).
            var movement = new MaterialMovement
            {
                MaterialId = entry.MaterialId,
                Quantity = entry.Quantity,
                MaterialUnitId = entry.MaterialUnitId,
                FromLocationId = null,
                ToLocationId = entry.LocationId,
                ToPersonId = entry.PersonelId,
                BrandId = entry.BrandId,
                ModelId = entry.ModelId,
                MovementType = "Giriş",
                Operation = "Giriş",
                MovementDate = entry.EntryDate,
                Aciklama = "MaterialEntry eklendi",
                Olusturan = modifiedBy,
                OlusturmaTarihi = DateTime.Now
            };
            await db.MaterialMovement_Table.AddAsync(movement, cancellationToken);

            // 4) IsAsset ise her birim için MaterialAsset üret.
            var material = await db.Material_Table
                .FirstOrDefaultAsync(m => m.Id == entry.MaterialId, cancellationToken);
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
                        SourceMaterialEntryId = entry.Id,
                        Aciklama = $"MaterialEntry #{entry.Id} ile otomatik oluşturuldu",
                        Olusturan = modifiedBy,
                        OlusturmaTarihi = DateTime.Now
                    };
                    await db.MaterialAsset_Table.AddAsync(asset, cancellationToken);
                }
            }

            await scope.CommitAsync(cancellationToken);
            return Result.Success();
            }
            catch (InvalidOperationException ex)
            {
                _logService?.LogException(ex, $"{Source}.AddAsync");
                return Result.Failure(Error.Validation(ex.Message, "MaterialEntry.Add.Invalid"));
            }
            catch (Exception ex)
            {
                _logService?.LogException(ex, $"{Source}.AddAsync");
                return Result.Failure(Error.Unexpected("Giriş kaydı eklenemedi.", ex, "MaterialEntry.Add.Failed"));
            }
        }

        public async Task<Result> UpdateMaterialEntryAndInventoryAsync(MaterialEntry entry, string? currentUserName, CancellationToken cancellationToken = default)
        {
            if (entry == null)
                return Result.Failure(Error.Validation("Giriş kaydı boş olamaz.", "MaterialEntry.Null"));
            try
            {
            currentUserName ??= Environment.UserName;

            // Canary (Adım 3): tek context + tek transaction içinde
            //   1) Eski entry'yi oku (değişiklik tespiti için)
            //   2) Inventory'i mevcut davranışa göre güncelle:
            //      - Sadece Quantity değişmişse: tek satıra delta uygula
            //      - Anahtar değişmişse: eski satırdan -oldQty, yeni satıra +newQty
            //   3) MaterialEntry satırını güncelle
            //   4) MaterialAsset senkronu (Tech-Debt #1 — Phase A):
            //      bu entry'den doğmuş ve hâlâ "el değmemiş" asset'ler güncellenir/eklenir/silinir.
            //   5) MaterialMovement "Düzeltme" / "Düzenleme" logu ekle
            await using var scope = await _scopeFactory.CreateAsync(cancellationToken);
            var db = scope.Context;

            var oldEntry = await db.MaterialEntry_Table
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == entry.Id, cancellationToken);
            if (oldEntry == null) throw new InvalidOperationException("Eski kayıt bulunamadı.");

            int? newLocation = entry.LocationId != 0 ? entry.LocationId : null;
            int? newPersonnel = (entry.PersonelId.HasValue && entry.PersonelId.Value != 0) ? entry.PersonelId : null;
            int? newBrand = (entry.BrandId.HasValue && entry.BrandId.Value != 0) ? entry.BrandId : null;
            int? newModel = (entry.ModelId.HasValue && entry.ModelId.Value != 0) ? entry.ModelId : null;

            int? oldLocation = oldEntry.LocationId != 0 ? oldEntry.LocationId : null;
            int? oldPersonnel = (oldEntry.PersonelId.HasValue && oldEntry.PersonelId.Value != 0) ? oldEntry.PersonelId : null;
            int? oldBrand = (oldEntry.BrandId.HasValue && oldEntry.BrandId.Value != 0) ? oldEntry.BrandId : null;
            int? oldModel = (oldEntry.ModelId.HasValue && oldEntry.ModelId.Value != 0) ? oldEntry.ModelId : null;

            // Değişiklik tespiti normalize edilmiş (0 → null) anahtar üzerinden yapılır;
            // aksi halde "0 ↔ null" UI/DB tutarsızlığı yanlış-pozitif keyChanged üretir.
            bool quantityChanged = entry.Quantity != oldEntry.Quantity;
            bool materialChanged = entry.MaterialId != oldEntry.MaterialId;
            bool unitChanged = entry.MaterialUnitId != oldEntry.MaterialUnitId;
            bool locationChanged = newLocation != oldLocation;
            bool personnelChanged = newPersonnel != oldPersonnel;
            bool brandChanged = newBrand != oldBrand;
            bool modelChanged = newModel != oldModel;

            bool keyChanged = materialChanged || locationChanged || unitChanged || personnelChanged || brandChanged || modelChanged;

            if (quantityChanged && !keyChanged)
            {
                // Senaryo A: Sadece miktar değişti — tek satırda delta uygula.
                int delta = entry.Quantity - oldEntry.Quantity;
                await ApplyInventoryDeltaAsync(
                    db,
                    entry.MaterialId,
                    newLocation,
                    newPersonnel,
                    newBrand,
                    newModel,
                    delta,
                    "Kayıt güncellemesi (miktar değişikliği)",
                    currentUserName,
                    cancellationToken);
            }
            else if (keyChanged)
            {
                // Senaryo B: Anahtar değişti — eski satırdan düş, yeni satıra ekle.
                await ApplyInventoryDeltaAsync(
                    db,
                    oldEntry.MaterialId,
                    oldLocation,
                    oldPersonnel,
                    oldBrand,
                    oldModel,
                    -oldEntry.Quantity,
                    "Kayıt güncellemesi (eski stoktan düş)",
                    currentUserName,
                    cancellationToken);

                await ApplyInventoryDeltaAsync(
                    db,
                    entry.MaterialId,
                    newLocation,
                    newPersonnel,
                    newBrand,
                    newModel,
                    entry.Quantity,
                    "Kayıt güncellemesi (yeni stoğa ekle)",
                    currentUserName,
                    cancellationToken);
            }
            // else: hiçbir alan değişmedi — sadece entry update + movement (orijinal davranış).

            // 3) MaterialEntry satırını güncelle. Detached olabileceği için Update kullanıyoruz.
            db.MaterialEntry_Table.Update(entry);

            // 4) MaterialAsset senkronu (Tech-Debt #1 — Phase A).
            //    Sadece bu entry'den doğmuş (SourceMaterialEntryId == entry.Id) ve hareket görmemiş
            //    asset'ler güncellenir / eklenir / silinir. Hareket görmüş asset'ler hiç dokunulmaz.
            await SyncAssetsForEntryUpdateAsync(
                db,
                entry,
                oldEntry,
                newLocation,
                newPersonnel,
                newBrand,
                newModel,
                oldLocation,
                oldPersonnel,
                oldBrand,
                oldModel,
                quantityChanged,
                materialChanged,
                locationChanged,
                personnelChanged,
                brandChanged,
                modelChanged,
                currentUserName,
                cancellationToken);

            // 5) Movement logu (orijinal AddMovementForEntryAsync ile aynı alanlar).
            string movementType = (quantityChanged && !keyChanged) ? "Düzeltme" : "Düzenleme";
            var movement = new MaterialMovement
            {
                MaterialId = entry.MaterialId,
                Quantity = entry.Quantity,
                MaterialUnitId = entry.MaterialUnitId,
                FromLocationId = null,
                ToLocationId = entry.LocationId,
                ToPersonId = entry.PersonelId,
                BrandId = entry.BrandId,
                ModelId = entry.ModelId,
                MovementType = movementType,
                Operation = movementType,
                MovementDate = entry.EntryDate,
                Aciklama = "MaterialEntry güncellendi",
                Olusturan = currentUserName,
                OlusturmaTarihi = DateTime.Now
            };
            await db.MaterialMovement_Table.AddAsync(movement, cancellationToken);

            await scope.CommitAsync(cancellationToken);
            return Result.Success();
            }
            catch (InvalidOperationException ex)
            {
                _logService?.LogException(ex, $"{Source}.UpdateMaterialEntryAndInventoryAsync");
                return Result.Failure(Error.Validation(ex.Message, "MaterialEntry.Update.Invalid"));
            }
            catch (Exception ex)
            {
                _logService?.LogException(ex, $"{Source}.UpdateMaterialEntryAndInventoryAsync");
                return Result.Failure(Error.Unexpected("Giriş kaydı güncellenemedi.", ex, "MaterialEntry.Update.Failed"));
            }
        }

        // Inventory upsert helper — orijinal AddOrUpdateInventoryAsync semantiği aynı context/transaction içinde.
        // 5'li anahtar (MaterialId + LocationId + PersonelId + BrandId + ModelId), NULL eşleşmeleri dahil.
        private static async Task ApplyInventoryDeltaAsync(
            ApplicationDbContext db,
            int materialId,
            int? locationId,
            int? personelId,
            int? brandId,
            int? modelId,
            int delta,
            string aciklama,
            string? modifiedBy,
            CancellationToken cancellationToken)
        {
            var existing = await db.MaterialInventory_Table
                .FirstOrDefaultAsync(mi =>
                    mi.MaterialId == materialId &&
                    mi.LocationId == locationId &&
                    mi.PersonelId == personelId &&
                    mi.BrandId == brandId &&
                    mi.ModelId == modelId,
                    cancellationToken);

            if (existing != null)
            {
                var newQty = existing.Quantity + delta;
                if (newQty < 0)
                    throw new InvalidOperationException($"Yetersiz stok: mevcut {existing.Quantity}, yapılmak istenen değişiklik {delta}. İşlem yapılmadı.");

                existing.Quantity = newQty;
                existing.Aciklama = aciklama;
                existing.Degistiren = modifiedBy;
                existing.DegistirmeTarihi = DateTime.Now;
            }
            else
            {
                if (delta < 0)
                    throw new InvalidOperationException("Yeni bir stok kaydı eklendiğinde negatif miktar belirtilemez.");

                var inventory = new MaterialInventory
                {
                    MaterialId = materialId,
                    LocationId = locationId,
                    PersonelId = personelId,
                    BrandId = brandId,
                    ModelId = modelId,
                    Quantity = delta,
                    Aciklama = aciklama,
                    Olusturan = modifiedBy,
                    OlusturmaTarihi = DateTime.Now
                };
                await db.MaterialInventory_Table.AddAsync(inventory, cancellationToken);
            }
        }

        /// <summary>
        /// Tech-Debt #1 — Phase A: MaterialEntry düzenlendiğinde, sadece bu entry'den doğmuş
        /// (<see cref="MaterialAsset.SourceMaterialEntryId"/> == entry.Id) ve "el değmemiş"
        /// asset'leri senkronize eder. Hareket görmüş asset'lere asla dokunmaz.
        ///
        /// "El değmemiş" tanımı (defansif, iki katmanlı):
        ///   1) MaterialAssetLog_Table'da o asset için kayıt yoksa
        ///   2) Asset'in mevcut (LocationId, PersonelId, BrandId, ModelId) tuple'ı,
        ///      orijinal entry'nin (oldEntry) tuple'ı ile aynıysa.
        /// İkinci kural, log dışı bir akışın asset'i taşıması ihtimaline karşı ek güvencedir.
        /// </summary>
        private static async Task SyncAssetsForEntryUpdateAsync(
            ApplicationDbContext db,
            MaterialEntry entry,
            MaterialEntry oldEntry,
            int? newLocation,
            int? newPersonnel,
            int? newBrand,
            int? newModel,
            int? oldLocation,
            int? oldPersonnel,
            int? oldBrand,
            int? oldModel,
            bool quantityChanged,
            bool materialChanged,
            bool locationChanged,
            bool personnelChanged,
            bool brandChanged,
            bool modelChanged,
            string? modifiedBy,
            CancellationToken cancellationToken)
        {
            // Yalnızca asset üreten malzemeler için anlamlı; değilse hiç çalışma.
            var material = await db.Material_Table
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == entry.MaterialId, cancellationToken);
            if (material == null || !material.IsAsset)
                return;

            // Bu entry'den doğmuş ve henüz log görmemiş asset'leri çek.
            // (Hareket gören her asset MaterialAssetLog_Table'a yazıldığı için log yokluğu yeterli ölçüttür;
            //  yine de tuple eşleşmesini ek savunma katmanı olarak uygulayacağız.)
            var loggedAssetIds = await db.MaterialAssetLog_Table
                .AsNoTracking()
                .Select(l => l.MaterialAssetId)
                .Distinct()
                .ToListAsync(cancellationToken);

            var sourcedAssets = await db.MaterialAsset_Table
                .Where(a => a.SourceMaterialEntryId == entry.Id)
                .ToListAsync(cancellationToken);

            // El değmemiş alt küme: log görmemiş + tuple hâlâ eski entry ile aynı.
            var untouched = sourcedAssets
                .Where(a => !loggedAssetIds.Contains(a.Id))
                .Where(a =>
                    a.LocationId == oldLocation &&
                    a.PersonelId == oldPersonnel &&
                    a.BrandId == oldBrand &&
                    a.ModelId == oldModel &&
                    a.MaterialId == oldEntry.MaterialId)
                .ToList();

            // 1) Anahtar alan değişiklikleri (Material/Location/Personel/Brand/Model) — el değmemiş asset'lere uygula.
            if (materialChanged || locationChanged || personnelChanged || brandChanged || modelChanged)
            {
                foreach (var a in untouched)
                {
                    if (materialChanged) a.MaterialId = entry.MaterialId;
                    if (locationChanged) a.LocationId = newLocation;
                    if (personnelChanged) a.PersonelId = newPersonnel;
                    if (brandChanged) a.BrandId = newBrand;
                    if (modelChanged) a.ModelId = newModel;
                    a.Aciklama = $"MaterialEntry #{entry.Id} güncellendi (Phase A senkron)";
                    a.Degistiren = modifiedBy;
                    a.DegistirmeTarihi = DateTime.Now;
                }
            }

            // 2) Quantity değişikliği — sadece el değmemiş alt küme üzerinde insert/delete.
            if (quantityChanged)
            {
                int delta = entry.Quantity - oldEntry.Quantity;
                if (delta > 0)
                {
                    // Yeni asset'leri yeni anahtar değerleri ile üret.
                    for (int i = 0; i < delta; i++)
                    {
                        var asset = new MaterialAsset
                        {
                            MaterialId = entry.MaterialId,
                            BrandId = newBrand,
                            ModelId = newModel,
                            LocationId = newLocation,
                            PersonelId = newPersonnel,
                            PurchaseDate = entry.EntryDate,
                            Status = AssetStatus.Active,
                            SourceMaterialEntryId = entry.Id,
                            Aciklama = $"MaterialEntry #{entry.Id} güncellendi, miktar artışı (Phase A senkron)",
                            Olusturan = modifiedBy,
                            OlusturmaTarihi = DateTime.Now
                        };
                        await db.MaterialAsset_Table.AddAsync(asset, cancellationToken);
                    }
                }
                else if (delta < 0)
                {
                    int needed = -delta;
                    // En yeni el değmemiş asset'lerden başlayarak sil; el değmemiş yetersizse yetersiz olduğu kadar sil.
                    // Hareket görmüş asset'lere dokunmuyoruz; eksik kalırsa kullanıcı manuel temizlik yapar.
                    var toRemove = untouched
                        .OrderByDescending(a => a.OlusturmaTarihi ?? DateTime.MinValue)
                        .ThenByDescending(a => a.Id)
                        .Take(needed)
                        .ToList();

                    foreach (var a in toRemove)
                        db.MaterialAsset_Table.Remove(a);
                }
            }
        }

        public async Task<Result> DeleteMaterialEntryAndUpdateInventoryAsync(MaterialEntry entryToDelete, string? currentUserName, CancellationToken cancellationToken = default)
        {
            if (entryToDelete == null)
                return Result.Failure(Error.Validation("Giriş kaydı boş olamaz.", "MaterialEntry.Null"));
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
                await using var scope = await _scopeFactory.CreateAsync(cancellationToken);
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

                // 3.b) MaterialAsset temizliği (Tech-Debt #1 — Phase A):
                //      Bu entry'den doğmuş ve hareket görmemiş asset'leri sil.
                //      Hareket görmüş asset'ler dokunulmaz; FK ON DELETE SET NULL ile bağları kopar.
                var loggedAssetIds = await db.MaterialAssetLog_Table
                    .AsNoTracking()
                    .Select(l => l.MaterialAssetId)
                    .Distinct()
                    .ToListAsync(cancellationToken);

                var sourcedAssets = await db.MaterialAsset_Table
                    .Where(a => a.SourceMaterialEntryId == entryToDelete.Id)
                    .ToListAsync(cancellationToken);

                var untouched = sourcedAssets
                    .Where(a => !loggedAssetIds.Contains(a.Id))
                    .Where(a =>
                        a.LocationId == location &&
                        a.PersonelId == personnel &&
                        a.BrandId == brand &&
                        a.ModelId == model &&
                        a.MaterialId == entryToDelete.MaterialId)
                    .ToList();

                foreach (var a in untouched)
                    db.MaterialAsset_Table.Remove(a);

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
                return Result.Success();
            }
            catch (InvalidOperationException ex)
            {
                _logService?.LogException(ex, $"{Source}.DeleteMaterialEntryAndUpdateInventoryAsync");
                return Result.Failure(Error.Validation(ex.Message, "MaterialEntry.Delete.Invalid"));
            }
            catch (Exception ex)
            {
                _logService?.LogException(ex, $"{Source}.DeleteMaterialEntryAndUpdateInventoryAsync");
                return Result.Failure(Error.Unexpected("Giriş kaydı silinemedi.", ex, "MaterialEntry.Delete.Failed"));
            }
        }

        public async Task<Result<bool>> AnyAsync(Expression<Func<MaterialEntry, bool>> predicate)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync();
                var any = await db.MaterialEntry_Table.AsNoTracking().AnyAsync(predicate);
                return Result.Success(any);
            }
            catch (Exception ex)
            {
                _logService?.LogException(ex, $"{Source}.AnyAsync");
                return Result.Failure<bool>(Error.Unexpected("Giriş kaydı sorgusu yapılamadı.", ex, "MaterialEntry.Any.Failed"));
            }
        }
    }
}