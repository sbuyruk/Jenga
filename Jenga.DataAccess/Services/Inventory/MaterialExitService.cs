using Jenga.DataAccess.Data;
using Jenga.Models.Enums;
using Jenga.Models.Inventory;
using Jenga.Models.Helpers;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.Inventory
{
    public class MaterialExitService : IMaterialExitService
    {
        private const string Source = nameof(MaterialExitService);

        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly IDbContextScopeFactory _scopeFactory;
        private readonly ILogService _logService;

        public MaterialExitService(IDbContextFactory<ApplicationDbContext> dbFactory, IDbContextScopeFactory scopeFactory, ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        }

        public async Task<Result<List<MaterialExit>>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.MaterialExit_Table.AsNoTracking().ToListAsync(cancellationToken);
                return Result.Success(list);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.GetAllAsync");
                return Result.Failure<List<MaterialExit>>(Error.Unexpected("Çikis kayitlari alinamadi.", ex, "MaterialExit.GetAll.Failed"));
            }
        }

        public async Task<Result> AddAsync(MaterialExit exit, List<int>? selectedAssetIds = null, CancellationToken cancellationToken = default)
        {
            if (exit == null)
                return Result.Failure(Error.Validation("Çikis kaydi bos olamaz.", "MaterialExit.Null"));
            try
            {

            int? actualLocation = exit.LocationId != 0 ? exit.LocationId : null;
            int? actualPerson = (exit.PersonelId.HasValue && exit.PersonelId.Value != 0) ? exit.PersonelId : null;
            int? actualBrand = (exit.BrandId.HasValue && exit.BrandId.Value != 0) ? exit.BrandId : null;
            int? actualModel = (exit.ModelId.HasValue && exit.ModelId.Value != 0) ? exit.ModelId : null;

            // Asama B: tek context + tek transaction içinde
            //   1) MaterialExit insert
            //   2) Material lookup (validate + IsAsset / unit)
            //   3) Inventory: -Quantity uygula (AddOrUpdate semantigi; negatife düserse hata)
            //   4) MaterialMovement "Çikis" logu
            //   5) IsAsset ise seçili/uygun asset'leri Retired'a çek + log
            // Hata olursa transaction rollback eder; kismi yazim olusmaz.
            await using var scope = await _scopeFactory.CreateAsync(cancellationToken);
            var db = scope.Context;
            db.SetCurrentUser(exit.Olusturan);
            await db.MaterialExit_Table.AddAsync(exit, cancellationToken);

            // 2) Material lookup
            var material = await db.Material_Table
                .FirstOrDefaultAsync(m => m.Id == exit.MaterialId, cancellationToken);
            if (material == null) throw new InvalidOperationException("Malzeme bulunamadi!");

            // 3) Inventory upsert (AddOrUpdateInventoryAsync semantigi birebir korundu).
            var inventory = await db.MaterialInventory_Table
                .FirstOrDefaultAsync(mi =>
                    mi.MaterialId == exit.MaterialId &&
                    mi.LocationId == actualLocation &&
                    mi.PersonelId == actualPerson &&
                    mi.BrandId == actualBrand &&
                    mi.ModelId == actualModel,
                    cancellationToken);

            int delta = -exit.Quantity;
            string invAciklama = $"MaterialExit: {exit.ExitType} islemi ile stoktan çikarildi.";
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

                var inv = new MaterialInventory
                {
                    MaterialId = exit.MaterialId,
                    LocationId = actualLocation,
                    PersonelId = actualPerson,
                    BrandId = actualBrand,
                    ModelId = actualModel,
                    Quantity = delta,
                    Aciklama = invAciklama
                };
                await db.MaterialInventory_Table.AddAsync(inv, cancellationToken);
            }

            // 4) Movement
            string operation = EnumHelper.GetEnumDescription((MaterialExitType)exit.ExitType.Value);
            var movement = new MaterialMovement
            {
                MaterialId = exit.MaterialId,
                Quantity = -exit.Quantity,
                MaterialUnitId = material.MaterialUnitId,
                FromLocationId = actualLocation,
                ToLocationId = null,
                FromPersonId = actualPerson,
                ToPersonId = null,
                MovementDate = exit.ExitDate,
                MovementType = "Çikis",
                Operation = $"Çikis nedeni: {operation}",
                Aciklama = $"MaterialExit: {operation} islemi.",
                BrandId = actualBrand,
                ModelId = actualModel
            };
            await db.MaterialMovement_Table.AddAsync(movement, cancellationToken);

            // 5) Asset retire (yalnizca IsAsset)
            if (material.IsAsset)
            {
                List<MaterialAsset> assetsToRetire;

                if (selectedAssetIds != null && selectedAssetIds.Count > 0)
                {
                    assetsToRetire = await db.MaterialAsset_Table
                        .Where(a => a.MaterialId == exit.MaterialId
                                 && selectedAssetIds.Contains(a.Id)
                                 && a.Status == AssetStatus.Active)
                        .ToListAsync(cancellationToken);
                }
                else
                {
                    assetsToRetire = await db.MaterialAsset_Table
                        .Where(a => a.MaterialId == exit.MaterialId
                                 && a.Status == AssetStatus.Active
                                 && a.LocationId == actualLocation
                                 && a.PersonelId == actualPerson
                                 && a.BrandId == actualBrand
                                 && a.ModelId == actualModel)
                        .Take(exit.Quantity)
                        .ToListAsync(cancellationToken);
                }

                foreach (var asset in assetsToRetire)
                {
                    var log = new MaterialAssetLog
                    {
                        MaterialAssetId = asset.Id,
                        FromPersonelId = asset.PersonelId,
                        ToPersonelId = null,
                        FromLocationId = asset.LocationId,
                        ToLocationId = null,
                        TransactionDate = DateTime.Now,
                        TransactionType = $"Çikis ({operation})",
                        Aciklama = $"Çikis: {asset.SerialNumber ?? asset.Id.ToString()} — {operation}"
                    };
                    await db.MaterialAssetLog_Table.AddAsync(log, cancellationToken);

                    asset.Status = AssetStatus.Retired;
                    asset.PersonelId = null;
                    asset.LocationId = null;
                }
            }

            await scope.CommitAsync(cancellationToken);
            return Result.Success();
            }
            catch (InvalidOperationException ex)
            {
                _logService.LogException(ex, $"{Source}.AddAsync");
                return Result.Failure(Error.Validation(ex.Message, "MaterialExit.Add.Invalid"));
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.AddAsync");
                return Result.Failure(Error.Unexpected("Çikis kaydi eklenemedi.", ex, "MaterialExit.Add.Failed"));
            }
        }

        public async Task<Result> UpdateAsync(MaterialExit newExit, CancellationToken cancellationToken = default)
        {
            if (newExit == null)
                return Result.Failure(Error.Validation("Çikis kaydi bos olamaz.", "MaterialExit.Null"));
            try
            {

            // Asama B: tek context + tek transaction içinde
            //   1) Eski exit'i oku
            //   2) Eski koordinatlara +oldQty (restore)
            //   3) Yeni koordinatlara -newQty (apply)
            //   4) "Düzeltme" movement logu
            //   5) Exit row update
            // Hata olursa hepsi rollback olur.
            await using var scope = await _scopeFactory.CreateAsync(cancellationToken);
            var db = scope.Context;
            db.SetCurrentUser(newExit.Olusturan);

            // 1) Eski exit (tracked olmali ki sonra alanlari güncelleyebilelim)
            var oldExit = await db.MaterialExit_Table
                .FirstOrDefaultAsync(e => e.Id == newExit.Id, cancellationToken);
            if (oldExit == null) throw new InvalidOperationException("Kayit bulunamadi!");

            int? oldLocation = oldExit.LocationId != 0 ? oldExit.LocationId : null;
            int? oldPerson = (oldExit.PersonelId.HasValue && oldExit.PersonelId.Value != 0) ? oldExit.PersonelId : null;
            int? oldBrand = (oldExit.BrandId.HasValue && oldExit.BrandId.Value != 0) ? oldExit.BrandId : null;
            int? oldModel = (oldExit.ModelId.HasValue && oldExit.ModelId.Value != 0) ? oldExit.ModelId : null;

            int? newLocation = newExit.LocationId != 0 ? newExit.LocationId : null;
            int? newPerson = (newExit.PersonelId.HasValue && newExit.PersonelId.Value != 0) ? newExit.PersonelId : null;
            int? newBrand = (newExit.BrandId.HasValue && newExit.BrandId.Value != 0) ? newExit.BrandId : null;
            int? newModel = (newExit.ModelId.HasValue && newExit.ModelId.Value != 0) ? newExit.ModelId : null;

            // 2) Eski koordinatlara +oldQty
            await ApplyInventoryDeltaAsync(
                db,
                oldExit.MaterialId, oldLocation, oldPerson, oldBrand, oldModel,
                +oldExit.Quantity,
                "MaterialExit güncellendi (eski miktar stokta geri eklendi)",
                newExit.Olusturan,
                cancellationToken);

            // 3) Yeni koordinatlara -newQty
            await ApplyInventoryDeltaAsync(
                db,
                newExit.MaterialId, newLocation, newPerson, newBrand, newModel,
                -newExit.Quantity,
                "MaterialExit güncellendi (yeni miktar stoktan çikarildi)",
                newExit.Olusturan,
                cancellationToken);

            // 4) Düzeltme movement
            string operation = EnumHelper.GetEnumDescription((MaterialExitType)newExit.ExitType.Value);
            var movement = new MaterialMovement
            {
                MaterialId = newExit.MaterialId,
                Quantity = -newExit.Quantity,
                MaterialUnitId = newExit.MaterialUnitId,
                FromLocationId = newExit.LocationId,
                ToPersonId = newExit.PersonelId,
                MovementDate = newExit.ExitDate,
                MovementType = "Düzeltme",
                Operation = $"Çikis nedeni: {operation}",
                Aciklama = "MaterialExit güncellendi.",
                BrandId = newBrand,
                ModelId = newModel
            };
            await db.MaterialMovement_Table.AddAsync(movement, cancellationToken);

            // 5) Exit row update — tracked entity üzerine alanlari kopyala (PK degismez)
            oldExit.MaterialId = newExit.MaterialId;
            oldExit.MaterialUnitId = newExit.MaterialUnitId;
            oldExit.Quantity = newExit.Quantity;
            oldExit.LocationId = newExit.LocationId;
            oldExit.PersonelId = newExit.PersonelId;
            oldExit.BrandId = newExit.BrandId;
            oldExit.ModelId = newExit.ModelId;
            oldExit.ExitDate = newExit.ExitDate;
            oldExit.ExitType = newExit.ExitType;
            oldExit.Aciklama = newExit.Aciklama;

            await scope.CommitAsync(cancellationToken);
            return Result.Success();
            }
            catch (InvalidOperationException ex)
            {
                _logService.LogException(ex, $"{Source}.UpdateAsync");
                return Result.Failure(Error.Validation(ex.Message, "MaterialExit.Update.Invalid"));
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.UpdateAsync");
                return Result.Failure(Error.Unexpected("Çikis kaydi güncellenemedi.", ex, "MaterialExit.Update.Failed"));
            }
        }

        // AddOrUpdateInventoryAsync semantigini birebir korur, ama PARAMETRE OLARAK GELEN context'te çalisir;
        // dolayisiyla ayni transaction'a katilir. 0'a düsse bile satir SILINMEZ.
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
            var inventory = await db.MaterialInventory_Table
                .FirstOrDefaultAsync(mi =>
                    mi.MaterialId == materialId &&
                    mi.LocationId == locationId &&
                    mi.PersonelId == personelId &&
                    mi.BrandId == brandId &&
                    mi.ModelId == modelId,
                    cancellationToken);

            if (inventory != null)
            {
                var newQty = inventory.Quantity + delta;
                if (newQty < 0)
                    throw new InvalidOperationException($"Yetersiz stok: mevcut {inventory.Quantity}, yapilmak istenen degisiklik {delta}. Islem yapilmadi.");

                inventory.Quantity = newQty;
                inventory.Aciklama = aciklama;
            }
            else
            {
                if (delta < 0)
                    throw new InvalidOperationException("Yeni bir stok kaydi eklendiginde negatif miktar belirtilemez.");

                var inv = new MaterialInventory
                {
                    MaterialId = materialId,
                    LocationId = locationId,
                    PersonelId = personelId,
                    BrandId = brandId,
                    ModelId = modelId,
                    Quantity = delta,
                    Aciklama = aciklama
                };
                await db.MaterialInventory_Table.AddAsync(inv, cancellationToken);
            }
        }

        public async Task<Result> DeleteAsync(MaterialExit exit, CancellationToken cancellationToken = default)
        {
            if (exit == null)
                return Result.Failure(Error.Validation("Çikis kaydi bos olamaz.", "MaterialExit.Null"));
            try
            {

            int? location = exit.LocationId != 0 ? exit.LocationId : null;
            int? person = (exit.PersonelId.HasValue && exit.PersonelId.Value != 0) ? exit.PersonelId : null;
            int? brand = (exit.BrandId.HasValue && exit.BrandId.Value != 0) ? exit.BrandId : null;
            int? model = (exit.ModelId.HasValue && exit.ModelId.Value != 0) ? exit.ModelId : null;

            // Asama B: tek context + tek transaction içinde
            //   1) Inventory: +Quantity uygula (silinen çikis miktarini stoga geri ekle).
            //      Eski davranis: AddOrUpdateInventoryAsync semantigi — satir yoksa olusturur,
            //      varsa quantity'yi günceller; 0'a düsse de satir SILINMEZ (Entry'den farkli).
            //   2) MaterialMovement "Silme" logu ekle.
            //   3) MaterialExit satirini sil.
            // Hata olursa using sonu rollback eder; "stok geri eklendi ama exit kaldi" tutarsizligi olusmaz.
            await using var scope = await _scopeFactory.CreateAsync(cancellationToken);
            var db = scope.Context;
            db.SetCurrentUser(exit.Olusturan);

            // 1) Inventory satirini bul (5'li anahtar, NULL eslesmeleri dahil).
            var inventory = await db.MaterialInventory_Table
                .FirstOrDefaultAsync(mi =>
                    mi.MaterialId == exit.MaterialId &&
                    mi.LocationId == location &&
                    mi.PersonelId == person &&
                    mi.BrandId == brand &&
                    mi.ModelId == model,
                    cancellationToken);

            int delta = exit.Quantity;
            if (inventory != null)
            {
                var newQty = inventory.Quantity + delta;
                if (newQty < 0)
                    throw new InvalidOperationException($"Yetersiz stok: mevcut {inventory.Quantity}, yapilmak istenen degisiklik {delta}. Islem yapilmadi.");

                inventory.Quantity = newQty;
                inventory.Aciklama = "MaterialExit silindi, stok geri eklendi.";
            }
            else
            {
                if (delta < 0)
                    throw new InvalidOperationException("Yeni bir stok kaydi eklendiginde negatif miktar belirtilemez.");

                var inv = new MaterialInventory
                {
                    MaterialId = exit.MaterialId,
                    LocationId = location,
                    PersonelId = person,
                    BrandId = brand,
                    ModelId = model,
                    Quantity = delta,
                    Aciklama = "MaterialExit silindi, stok geri eklendi."
                };
                await db.MaterialInventory_Table.AddAsync(inv, cancellationToken);
            }

            // 2) MaterialMovement "Silme" logu (orijinal alanlar birebir korundu).
            var movement = new MaterialMovement
            {
                MaterialId = exit.MaterialId,
                Quantity = exit.Quantity,
                MaterialUnitId = exit.MaterialUnitId,
                FromLocationId = exit.LocationId,
                ToPersonId = exit.PersonelId,
                MovementDate = DateTime.Now,
                MovementType = "Silme",
                Operation = "Silme",
                Aciklama = "MaterialExit silindi.",
                BrandId = brand,
                ModelId = model
            };
            await db.MaterialMovement_Table.AddAsync(movement, cancellationToken);

            // 3) MaterialExit satirini sil. Detached olabilecegi için tracked entity üzerinden Remove.
            var exitEntity = await db.MaterialExit_Table
                .FirstOrDefaultAsync(e => e.Id == exit.Id, cancellationToken);
            if (exitEntity != null)
                db.MaterialExit_Table.Remove(exitEntity);

            await scope.CommitAsync(cancellationToken);
            return Result.Success();
            }
            catch (InvalidOperationException ex)
            {
                _logService.LogException(ex, $"{Source}.DeleteAsync");
                return Result.Failure(Error.Validation(ex.Message, "MaterialExit.Delete.Invalid"));
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.DeleteAsync");
                return Result.Failure(Error.Unexpected("Çikis kaydi silinemedi.", ex, "MaterialExit.Delete.Failed"));
            }
        }

        public async Task<Result<bool>> AnyAsync(Expression<Func<MaterialExit, bool>> predicate)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync();
                var any = await db.MaterialExit_Table.AsNoTracking().AnyAsync(predicate);
                return Result.Success(any);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.AnyAsync");
                return Result.Failure<bool>(Error.Unexpected("Çikis sorgusu yapilamadi.", ex, "MaterialExit.Any.Failed"));
            }
        }
    }
}