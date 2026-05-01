using Jenga.DataAccess.Data;
using Jenga.Models.Inventory;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.Inventory
{
    public class MaterialTransferService : IMaterialTransferService
    {
        private const string Source = nameof(MaterialTransferService);

        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService _logService;

        public MaterialTransferService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService;
        }

        public async Task<Result> AddAsync(
            MaterialTransfer transfer,
            string? modifiedBy = null,
            List<int>? selectedAssetIds = null,
            CancellationToken cancellationToken = default)
        {
            if (transfer == null)
                return Result.Failure(Error.Validation("Transfer kaydı boş olamaz.", "MaterialTransfer.Null"));
            try
            {

            int? actualFromLocation = transfer.FromLocationId != 0 ? transfer.FromLocationId : null;
            int? actualToLocation = transfer.ToLocationId != 0 ? transfer.ToLocationId : null;
            int? actualFromPerson = (transfer.FromPersonId.HasValue && transfer.FromPersonId != 0) ? transfer.FromPersonId : null;
            int? actualToPerson = (transfer.ToPersonId.HasValue && transfer.ToPersonId != 0) ? transfer.ToPersonId : null;
            int? actualBrand = (transfer.BrandId.HasValue && transfer.BrandId != 0) ? transfer.BrandId : null;
            int? actualModel = (transfer.ModelId.HasValue && transfer.ModelId != 0) ? transfer.ModelId : null;

            // Aşama B: tek context + tek transaction içinde
            //   1) MaterialTransfer insert
            //   2) Inventory: kaynak koordinatlarına -Quantity (AddOrUpdate semantiği; 0'a düşse de satır silinmez)
            //   3) Inventory: hedef koordinatlarına +Quantity
            //   4) MaterialMovement "Transfer" logu
            //   5) IsAsset ise seçili/uygun asset'leri yeni location/person'a taşı + log
            // Hata olursa transaction rollback eder.
            await using var scope = await DbContextScope.CreateAsync(_dbFactory, cancellationToken);
            var db = scope.Context;

            // 1) Transfer insert
            await db.MaterialTransfer_Table.AddAsync(transfer, cancellationToken);

            // 2) Kaynak stoktan düş
            await ApplyInventoryDeltaAsync(
                db,
                transfer.MaterialId, actualFromLocation, actualFromPerson, actualBrand, actualModel,
                -transfer.Quantity,
                "MaterialTransfer: Kaynak stoktan düşüldü.",
                modifiedBy,
                cancellationToken);

            // 3) Hedef stoğa ekle
            await ApplyInventoryDeltaAsync(
                db,
                transfer.MaterialId, actualToLocation, actualToPerson, actualBrand, actualModel,
                +transfer.Quantity,
                "MaterialTransfer: Hedef stoğa eklendi.",
                modifiedBy,
                cancellationToken);

            // 4) Movement
            var movement = new MaterialMovement
            {
                MaterialId = transfer.MaterialId,
                Quantity = transfer.Quantity,
                MaterialUnitId = transfer.MaterialUnitId,
                FromLocationId = actualFromLocation ?? transfer.FromLocationId,
                ToLocationId = actualToLocation ?? transfer.ToLocationId,
                FromPersonId = actualFromPerson,
                ToPersonId = actualToPerson,
                MovementDate = transfer.TransferDate,
                MovementType = "Transfer",
                Aciklama = transfer.Aciklama ?? "MaterialTransfer işlemi",
                Olusturan = modifiedBy,
                OlusturmaTarihi = DateTime.Now,
                BrandId = actualBrand,
                ModelId = actualModel
            };
            await db.MaterialMovement_Table.AddAsync(movement, cancellationToken);

            // 5) Asset transfer (yalnızca IsAsset)
            var material = await db.Material_Table
                .FirstOrDefaultAsync(m => m.Id == transfer.MaterialId, cancellationToken);

            if (material != null && material.IsAsset)
            {
                List<MaterialAsset> assetsToTransfer;

                if (selectedAssetIds != null && selectedAssetIds.Count > 0)
                {
                    assetsToTransfer = await db.MaterialAsset_Table
                        .Where(a => a.MaterialId == transfer.MaterialId
                                 && selectedAssetIds.Contains(a.Id)
                                 && a.Status == AssetStatus.Active)
                        .ToListAsync(cancellationToken);
                }
                else
                {
                    assetsToTransfer = await db.MaterialAsset_Table
                        .Where(a => a.MaterialId == transfer.MaterialId
                                 && a.Status == AssetStatus.Active
                                 && a.LocationId == actualFromLocation
                                 && a.PersonelId == actualFromPerson
                                 && a.BrandId == actualBrand
                                 && a.ModelId == actualModel)
                        .Take(transfer.Quantity)
                        .ToListAsync(cancellationToken);
                }

                foreach (var asset in assetsToTransfer)
                {
                    var log = new MaterialAssetLog
                    {
                        MaterialAssetId = asset.Id,
                        FromPersonelId = asset.PersonelId,
                        ToPersonelId = actualToPerson,
                        FromLocationId = asset.LocationId,
                        ToLocationId = actualToLocation,
                        TransactionDate = DateTime.Now,
                        TransactionType = "Transfer",
                        Aciklama = $"Transfer #{asset.SerialNumber ?? asset.Id.ToString()}",
                        Olusturan = modifiedBy,
                        OlusturmaTarihi = DateTime.Now
                    };
                    await db.MaterialAssetLog_Table.AddAsync(log, cancellationToken);

                    asset.LocationId = actualToLocation;
                    asset.PersonelId = actualToPerson;
                    asset.Degistiren = modifiedBy;
                    asset.DegistirmeTarihi = DateTime.Now;
                }
            }

            await scope.CommitAsync(cancellationToken);
            return Result.Success();
            }
            catch (InvalidOperationException ex)
            {
                _logService?.LogException(ex, $"{Source}.AddAsync");
                return Result.Failure(Error.Validation(ex.Message, "MaterialTransfer.Add.Invalid"));
            }
            catch (Exception ex)
            {
                _logService?.LogException(ex, $"{Source}.AddAsync");
                return Result.Failure(Error.Unexpected("Transfer kaydı eklenemedi.", ex, "MaterialTransfer.Add.Failed"));
            }
        }

        // AddOrUpdateInventoryAsync semantiğini birebir korur, parametre olarak gelen
        // context'te çalışır; aynı transaction'a katılır. 0'a düşse bile satır SİLİNMEZ.
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
                    throw new InvalidOperationException($"Yetersiz stok: mevcut {inventory.Quantity}, yapılmak istenen değişiklik {delta}. İşlem yapılmadı.");

                inventory.Quantity = newQty;
                inventory.Aciklama = aciklama;
                inventory.Degistiren = modifiedBy;
                inventory.DegistirmeTarihi = DateTime.Now;
            }
            else
            {
                if (delta < 0)
                    throw new InvalidOperationException("Yeni bir stok kaydı eklendiğinde negatif miktar belirtilemez.");

                var inv = new MaterialInventory
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
                await db.MaterialInventory_Table.AddAsync(inv, cancellationToken);
            }
        }
    }
}
