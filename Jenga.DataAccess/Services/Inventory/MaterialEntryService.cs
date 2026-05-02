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
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
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
                _logService.LogException(ex, $"{Source}.GetAllAsync");
                return Result.Failure<List<MaterialEntry>>(Error.Unexpected("Giris kayitlari alinamadi.", ex, "MaterialEntry.GetAll.Failed"));
            }
        }

        public async Task<Result> AddAsync(MaterialEntry entry, string? modifiedBy = null, CancellationToken cancellationToken = default)
        {
            if (entry == null)
                return Result.Failure(Error.Validation("Giris kaydi bos olamaz.", "MaterialEntry.Null"));
            try
            {

            int? actualLocationId = entry.LocationId != 0 ? entry.LocationId : null;
            int? actualPersonnelId = (entry.PersonelId.HasValue && entry.PersonelId.Value != 0) ? entry.PersonelId : null;
            int? actualBrandId = (entry.BrandId.HasValue && entry.BrandId.Value != 0) ? entry.BrandId : null;
            int? actualModelId = (entry.ModelId.HasValue && entry.ModelId.Value != 0) ? entry.ModelId : null;

            // Canary (Adim 2): tek context + tek transaction içinde
            //   1) MaterialEntry ekle ve Id'yi al
            //   2) Inventory: +Quantity uygula (yoksa yeni satir)
            //   3) MaterialMovement "Giris" logu ekle
            //   4) Material.IsAsset ise N adet MaterialAsset üret
            // Hata olursa using sonu rollback eder; "entry kaldi ama inventory/movement/asset eksik" tutarsizligi olusmaz.
            await using var scope = await _scopeFactory.CreateAsync(cancellationToken);
            var db = scope.Context;
            db.SetCurrentUser(modifiedBy);
            //    ayni transaction içinde oldugundan rollback'i engellemez.
            await db.MaterialEntry_Table.AddAsync(entry, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);

            // 2) Inventory upsert (5'li anahtar, NULL eslesmeleri dahil) — orijinal AddOrUpdateInventoryAsync ile ayni semantik.
            var inventory = await db.MaterialInventory_Table
                .FirstOrDefaultAsync(mi =>
                    mi.MaterialId == entry.MaterialId &&
                    mi.LocationId == actualLocationId &&
                    mi.PersonelId == actualPersonnelId &&
                    mi.BrandId == actualBrandId &&
                    mi.ModelId == actualModelId,
                    cancellationToken);

            int delta = entry.Quantity;
            const string invAciklama = "Malzeme girisi sonrasi stok güncellemesi";
            if (inventory != null)
            {
                var newQty = inventory.Quantity + delta;
                if (newQty < 0)
                    throw new InvalidOperationException($"Yetersiz stok: mevcut {inventory.Quantity}, yapilmak istenen degisiklik {delta}. Islem yapilmadi.");

                inventory.Quantity = newQty;
                inventory.Aciklama = invAciklama;
            }
            else
            {
                if (delta < 0)
                    throw new InvalidOperationException("Yeni bir stok kaydi eklendiginde negatif miktar belirtilemez.");

                var newInventory = new MaterialInventory
                {
                    MaterialId = entry.MaterialId,
                    LocationId = actualLocationId,
                    PersonelId = actualPersonnelId,
                    BrandId = actualBrandId,
                    ModelId = actualModelId,
                    Quantity = delta,
                    Aciklama = invAciklama
                };
                await db.MaterialInventory_Table.AddAsync(newInventory, cancellationToken);
            }

            // 3) MaterialMovement "Giris" logu (orijinal AddMovementForEntryAsync ile ayni alanlar).
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
                MovementType = "Giris",
                Operation = "Giris",
                MovementDate = entry.EntryDate,
                Aciklama = "MaterialEntry eklendi"
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
                        Aciklama = $"MaterialEntry #{entry.Id} ile otomatik olusturuldu"
                    };
                    await db.MaterialAsset_Table.AddAsync(asset, cancellationToken);
                }
            }

            await scope.CommitAsync(cancellationToken);
            return Result.Success();
            }
            catch (InvalidOperationException ex)
            {
                _logService.LogException(ex, $"{Source}.AddAsync");
                return Result.Failure(Error.Validation(ex.Message, "MaterialEntry.Add.Invalid"));
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.AddAsync");
                return Result.Failure(Error.Unexpected("Giris kaydi eklenemedi.", ex, "MaterialEntry.Add.Failed"));
            }
        }

        public async Task<Result> UpdateMaterialEntryAndInventoryAsync(MaterialEntry entry, string? currentUserName, CancellationToken cancellationToken = default)
        {
            if (entry == null)
                return Result.Failure(Error.Validation("Giris kaydi bos olamaz.", "MaterialEntry.Null"));
            try
            {
            // Canary (Adim 3):
            //   1) Eski entry'yi oku (degisiklik tespiti için)
            //   2) Inventory'i mevcut davranisa göre güncelle:
            //      - Sadece Quantity degismisse: tek satira delta uygula
            //      - Anahtar degismisse: eski satirdan -oldQty, yeni satira +newQty
            //   3) MaterialEntry satirini güncelle
            //   4) MaterialAsset senkronu (Tech-Debt #1 — Phase A):
            //      bu entry'den dogmus ve hâlâ "el degmemis" asset'ler güncellenir/eklenir/silinir.
            //   5) MaterialMovement "Düzeltme" / "Düzenleme" logu ekle
            await using var scope = await _scopeFactory.CreateAsync(cancellationToken);
            var db = scope.Context;
            db.SetCurrentUser(currentUserName);

            var oldEntry = await db.MaterialEntry_Table
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == entry.Id, cancellationToken);
            if (oldEntry == null) throw new InvalidOperationException("Eski kayit bulunamadi.");

            int? newLocation = entry.LocationId != 0 ? entry.LocationId : null;
            int? newPersonnel = (entry.PersonelId.HasValue && entry.PersonelId.Value != 0) ? entry.PersonelId : null;
            int? newBrand = (entry.BrandId.HasValue && entry.BrandId.Value != 0) ? entry.BrandId : null;
            int? newModel = (entry.ModelId.HasValue && entry.ModelId.Value != 0) ? entry.ModelId : null;

            int? oldLocation = oldEntry.LocationId != 0 ? oldEntry.LocationId : null;
            int? oldPersonnel = (oldEntry.PersonelId.HasValue && oldEntry.PersonelId.Value != 0) ? oldEntry.PersonelId : null;
            int? oldBrand = (oldEntry.BrandId.HasValue && oldEntry.BrandId.Value != 0) ? oldEntry.BrandId : null;
            int? oldModel = (oldEntry.ModelId.HasValue && oldEntry.ModelId.Value != 0) ? oldEntry.ModelId : null;

            // Degisiklik tespiti normalize edilmis (0 ? null) anahtar üzerinden yapilir;
            // aksi halde "0 ? null" UI/DB tutarsizligi yanlis-pozitif keyChanged üretir.
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
                // Senaryo A: Sadece miktar degisti — tek satirda delta uygula.
                int delta = entry.Quantity - oldEntry.Quantity;
                await ApplyInventoryDeltaAsync(
                    db,
                    entry.MaterialId,
                    newLocation,
                    newPersonnel,
                    newBrand,
                    newModel,
                    delta,
                    "Kayit güncellemesi (miktar degisikligi)",
                    currentUserName,
                    cancellationToken);
            }
            else if (keyChanged)
            {
                // Senaryo B: Anahtar degisti — eski satirdan düs, yeni satira ekle.
                await ApplyInventoryDeltaAsync(
                    db,
                    oldEntry.MaterialId,
                    oldLocation,
                    oldPersonnel,
                    oldBrand,
                    oldModel,
                    -oldEntry.Quantity,
                    "Kayit güncellemesi (eski stoktan düs)",
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
                    "Kayit güncellemesi (yeni stoga ekle)",
                    currentUserName,
                    cancellationToken);
            }
            // else: hiçbir alan degismedi — sadece entry update + movement (orijinal davranis).

            // 3) MaterialEntry satirini güncelle. Detached olabilecegi için Update kullaniyoruz.
            db.MaterialEntry_Table.Update(entry);

            // 4) MaterialAsset senkronu (Tech-Debt #1 — Phase A).
            //    Sadece bu entry'den dogmus (SourceMaterialEntryId == entry.Id) ve hareket görmemis
            //    asset'ler güncellenir / eklenir / silinir. Hareket görmüs asset'ler hiç dokunulmaz.
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

            // 5) Movement logu (orijinal AddMovementForEntryAsync ile ayni alanlar).
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
            };
            await db.MaterialMovement_Table.AddAsync(movement, cancellationToken);

            await scope.CommitAsync(cancellationToken);
            return Result.Success();
            }
            catch (InvalidOperationException ex)
            {
                _logService.LogException(ex, $"{Source}.UpdateMaterialEntryAndInventoryAsync");
                return Result.Failure(Error.Validation(ex.Message, "MaterialEntry.Update.Invalid"));
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.UpdateMaterialEntryAndInventoryAsync");
                return Result.Failure(Error.Unexpected("Giris kaydi güncellenemedi.", ex, "MaterialEntry.Update.Failed"));
            }
        }

        // Inventory upsert helper — orijinal AddOrUpdateInventoryAsync semantigi ayni context/transaction içinde.
        // 5'li anahtar (MaterialId + LocationId + PersonelId + BrandId + ModelId), NULL eslesmeleri dahil.
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
                    throw new InvalidOperationException($"Yetersiz stok: mevcut {existing.Quantity}, yapilmak istenen degisiklik {delta}. Islem yapilmadi.");

                existing.Quantity = newQty;
                existing.Aciklama = aciklama;
            }
            else
            {
                if (delta < 0)
                    throw new InvalidOperationException("Yeni bir stok kaydi eklendiginde negatif miktar belirtilemez.");

                var inventory = new MaterialInventory
                {
                    MaterialId = materialId,
                    LocationId = locationId,
                    PersonelId = personelId,
                    BrandId = brandId,
                    ModelId = modelId,
                    Quantity = delta,
                    Aciklama = aciklama
                };
                await db.MaterialInventory_Table.AddAsync(inventory, cancellationToken);
            }
        }

        /// <summary>
        /// Tech-Debt #1 — Phase A: MaterialEntry düzenlendiginde, sadece bu entry'den dogmus
        /// (<see cref="MaterialAsset.SourceMaterialEntryId"/> == entry.Id) ve "el degmemis"
        /// asset'leri senkronize eder. Hareket görmüs asset'lere asla dokunmaz.
        ///
        /// "El degmemis" tanimi (defansif, iki katmanli):
        ///   1) MaterialAssetLog_Table'da o asset için kayit yoksa
        ///   2) Asset'in mevcut (LocationId, PersonelId, BrandId, ModelId) tuple'i,
        ///      orijinal entry'nin (oldEntry) tuple'i ile ayniysa.
        /// Ikinci kural, log disi bir akisin asset'i tasimasi ihtimaline karsi ek güvencedir.
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
            // Yalnizca asset üreten malzemeler için anlamli; degilse hiç çalisma.
            var material = await db.Material_Table
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == entry.MaterialId, cancellationToken);
            if (material == null || !material.IsAsset)
                return;

            // Bu entry'den dogmus ve henüz log görmemis asset'leri çek.
            // (Hareket gören her asset MaterialAssetLog_Table'a yazildigi için log yoklugu yeterli ölçüttür;
            //  yine de tuple eslesmesini ek savunma katmani olarak uygulayacagiz.)
            var loggedAssetIds = await db.MaterialAssetLog_Table
                .AsNoTracking()
                .Select(l => l.MaterialAssetId)
                .Distinct()
                .ToListAsync(cancellationToken);

            var sourcedAssets = await db.MaterialAsset_Table
                .Where(a => a.SourceMaterialEntryId == entry.Id)
                .ToListAsync(cancellationToken);

            // El degmemis alt küme: log görmemis + tuple hâlâ eski entry ile ayni.
            var untouched = sourcedAssets
                .Where(a => !loggedAssetIds.Contains(a.Id))
                .Where(a =>
                    a.LocationId == oldLocation &&
                    a.PersonelId == oldPersonnel &&
                    a.BrandId == oldBrand &&
                    a.ModelId == oldModel &&
                    a.MaterialId == oldEntry.MaterialId)
                .ToList();

            // 1) Anahtar alan degisiklikleri (Material/Location/Personel/Brand/Model) — el degmemis asset'lere uygula.
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
                }
            }

            // 2) Quantity degisikligi — sadece el degmemis alt küme üzerinde insert/delete.
            if (quantityChanged)
            {
                int delta = entry.Quantity - oldEntry.Quantity;
                if (delta > 0)
                {
                    // Yeni asset'leri yeni anahtar degerleri ile üret.
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
                            Aciklama = $"MaterialEntry #{entry.Id} güncellendi, miktar artisi (Phase A senkron)",
                        };
                        await db.MaterialAsset_Table.AddAsync(asset, cancellationToken);
                    }
                }
                else if (delta < 0)
                {
                    int needed = -delta;
                    // En yeni el degmemis asset'lerden baslayarak sil; el degmemis yetersizse yetersiz oldugu kadar sil.
                    // Hareket görmüs asset'lere dokunmuyoruz; eksik kalirsa kullanici manuel temizlik yapar.
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
                return Result.Failure(Error.Validation("Giris kaydi bos olamaz.", "MaterialEntry.Null"));

            int? location = entryToDelete.LocationId != 0 ? entryToDelete.LocationId : null;
            int? personnel = (entryToDelete.PersonelId.HasValue && entryToDelete.PersonelId.Value != 0) ? entryToDelete.PersonelId : null;
            int? brand = (entryToDelete.BrandId.HasValue && entryToDelete.BrandId.Value != 0) ? entryToDelete.BrandId : null;
            int? model = (entryToDelete.ModelId.HasValue && entryToDelete.ModelId.Value != 0) ? entryToDelete.ModelId : null;

            // Canary (Adim 1): tek context + tek transaction içinde
            //   1) Inventory: -Quantity uygula (yoksa negatif ekleme yasak; orijinal davranis)
            //   2) Inventory satiri <= 0 düstüyse sil
            //   3) MaterialEntry satirini sil
            //   4) MaterialMovement "Silme" logu ekle
            // Hata olursa using sonu rollback eder; "stok düstü ama entry kaldi" gibi tutarsizliklar olusmaz.
            try
            {
                await using var scope = await _scopeFactory.CreateAsync(cancellationToken);
                var db = scope.Context;
                db.SetCurrentUser(currentUserName);

                // 1) Inventory satirini bul
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
                        throw new InvalidOperationException($"Yetersiz stok: mevcut {inventory.Quantity}, yapilmak istenen degisiklik {delta}. Islem yapilmadi.");

                    inventory.Quantity = newQty;
                    inventory.Aciklama = "MaterialEntry silindi, stoktan çikarildi";

                    // 2) Sifir veya altina düstüyse satiri kaldir.
                    if (newQty <= 0)
                        db.MaterialInventory_Table.Remove(inventory);
                }
                else
                {
                    // Mevcut servis davranisi: yeni satir eklerken negatif miktar yasak.
                    if (delta < 0)
                        throw new InvalidOperationException("Yeni bir stok kaydi eklendiginde negatif miktar belirtilemez.");
                }

                // 3) MaterialEntry satirini sil. Detached olabilecegi için Attach + Remove ile state'i kesinlestiriyoruz.
                var entryEntity = await db.MaterialEntry_Table
                    .FirstOrDefaultAsync(e => e.Id == entryToDelete.Id, cancellationToken);
                if (entryEntity != null)
                    db.MaterialEntry_Table.Remove(entryEntity);

                // 3.b) MaterialAsset temizligi (Tech-Debt #1 — Phase A):
                //      Bu entry'den dogmus ve hareket görmemis asset'leri sil.
                //      Hareket görmüs asset'ler dokunulmaz; FK ON DELETE SET NULL ile baglari kopar.
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

                // 4) MaterialMovement "Silme" logu (orijinal AddMovementForEntryAsync ile ayni alanlar).
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
                    Aciklama = "MaterialEntry silindi"
                };
                await db.MaterialMovement_Table.AddAsync(movement, cancellationToken);

                await scope.CommitAsync(cancellationToken);
                return Result.Success();
            }
            catch (InvalidOperationException ex)
            {
                _logService.LogException(ex, $"{Source}.DeleteMaterialEntryAndUpdateInventoryAsync");
                return Result.Failure(Error.Validation(ex.Message, "MaterialEntry.Delete.Invalid"));
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.DeleteMaterialEntryAndUpdateInventoryAsync");
                return Result.Failure(Error.Unexpected("Giris kaydi silinemedi.", ex, "MaterialEntry.Delete.Failed"));
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
                _logService.LogException(ex, $"{Source}.AnyAsync");
                return Result.Failure<bool>(Error.Unexpected("Giris kaydi sorgusu yapilamadi.", ex, "MaterialEntry.Any.Failed"));
            }
        }
    }
}