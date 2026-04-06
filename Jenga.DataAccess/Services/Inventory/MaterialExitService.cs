using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.Enums;
using Jenga.Models.Inventory;
using Jenga.Utility.Helpers;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.Inventory
{
    public class MaterialExitService : IMaterialExitService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMaterialInventoryService _materialInventoryService;
        private readonly IMaterialMovementService _materialMovementService;
        private readonly IMaterialAssetService _materialAssetService;
        private readonly IMaterialAssetLogService _materialAssetLogService;

        public MaterialExitService(
             IUnitOfWork unitOfWork,
             IMaterialInventoryService materialInventoryService,
             IMaterialMovementService materialMovementService,
             IMaterialAssetService materialAssetService,
             IMaterialAssetLogService materialAssetLogService)
        {
            _unitOfWork = unitOfWork;
            _materialInventoryService = materialInventoryService;
            _materialMovementService = materialMovementService;
            _materialAssetService = materialAssetService;
            _materialAssetLogService = materialAssetLogService;
        }

        public async Task<List<MaterialExit>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _unitOfWork.MaterialExit.GetAllAsync(cancellationToken);

        public async Task<MaterialExit?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => await _unitOfWork.MaterialExit.GetByIdAsync(id, cancellationToken);

        public async Task AddAsync(MaterialExit exit, List<int>? selectedAssetIds = null, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.MaterialExit.AddAsync(exit, cancellationToken);
            await _unitOfWork.MaterialExit.SaveChangesAsync(cancellationToken);

            var material = await _unitOfWork.Material.GetByIdAsync(exit.MaterialId, cancellationToken);
            if (material == null) throw new Exception("Malzeme bulunamadı!");

            int? actualLocation = exit.LocationId != 0 ? exit.LocationId : null;
            int? actualPerson = (exit.PersonelId.HasValue && exit.PersonelId.Value != 0) ? exit.PersonelId : null;
            int? actualBrand = (exit.BrandId.HasValue && exit.BrandId.Value != 0) ? exit.BrandId : null;
            int? actualModel = (exit.ModelId.HasValue && exit.ModelId.Value != 0) ? exit.ModelId : null;

            await _materialInventoryService.AddOrUpdateInventoryAsync(
                exit.MaterialId,
                actualLocation,
                actualPerson,
                -exit.Quantity,
                $"MaterialExit: {exit.ExitType} işlemi ile stoktan çıkarıldı.",
                exit.Olusturan,
                actualBrand,
                actualModel,
                cancellationToken);

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
                MovementType = "Çıkış",
                Operation = $"Çıkış nedeni: {operation}",
                Aciklama = $"MaterialExit: {operation} işlemi.",
                Olusturan = exit.Olusturan,
                OlusturmaTarihi = DateTime.Now,
                BrandId = actualBrand,
                ModelId = actualModel
            };
            await _materialMovementService.AddAsync(movement, cancellationToken);

            if (material.IsAsset)
            {
                await RetireAssetsAsync(
                    exit.MaterialId,
                    exit.Quantity,
                    actualLocation,
                    actualPerson,
                    actualBrand,
                    actualModel,
                    operation,
                    exit.Olusturan,
                    selectedAssetIds,
                    cancellationToken);
            }
        }

        private async Task RetireAssetsAsync(
            int materialId,
            int quantity,
            int? locationId,
            int? personelId,
            int? brandId,
            int? modelId,
            string exitReason,
            string? modifiedBy,
            List<int>? selectedAssetIds,
            CancellationToken cancellationToken)
        {
            List<MaterialAsset> assetsToRetire;

            if (selectedAssetIds != null && selectedAssetIds.Count > 0)
            {
                var allAssets = await _materialAssetService.GetByMaterialIdAsync(materialId, cancellationToken);
                assetsToRetire = allAssets
                    .Where(a => selectedAssetIds.Contains(a.Id) && a.Status == AssetStatus.Active)
                    .ToList();
            }
            else
            {
                var allAssets = await _materialAssetService.GetByMaterialIdAsync(materialId, cancellationToken);
                assetsToRetire = allAssets
                    .Where(a => a.Status == AssetStatus.Active
                        && a.LocationId == locationId
                        && a.PersonelId == personelId
                        && a.BrandId == brandId
                        && a.ModelId == modelId)
                    .Take(quantity)
                    .ToList();
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
                    TransactionType = $"Çıkış ({exitReason})",
                    Aciklama = $"Çıkış: {asset.SerialNumber ?? asset.Id.ToString()} — {exitReason}",
                    Olusturan = modifiedBy,
                    OlusturmaTarihi = DateTime.Now
                };
                await _materialAssetLogService.AddAsync(log, cancellationToken);

                asset.Status = AssetStatus.Retired;
                asset.PersonelId = null;
                asset.LocationId = null;
                asset.Degistiren = modifiedBy;
                asset.DegistirmeTarihi = DateTime.Now;
                await _materialAssetService.UpdateAsync(asset, cancellationToken);
            }
        }

        public async Task UpdateAsync(MaterialExit newExit, CancellationToken cancellationToken = default)
        {
            var oldExit = await GetByIdAsync(newExit.Id, cancellationToken);
            if (oldExit == null) throw new Exception("Kayıt bulunamadı!");

            int? oldLocation = oldExit.LocationId != 0 ? oldExit.LocationId : null;
            int? oldPerson = (oldExit.PersonelId.HasValue && oldExit.PersonelId.Value != 0) ? oldExit.PersonelId : null;
            int? oldBrand = (oldExit.BrandId.HasValue && oldExit.BrandId.Value != 0) ? oldExit.BrandId : null;
            int? oldModel = (oldExit.ModelId.HasValue && oldExit.ModelId.Value != 0) ? oldExit.ModelId : null;

            int? newLocation = newExit.LocationId != 0 ? newExit.LocationId : null;
            int? newPerson = (newExit.PersonelId.HasValue && newExit.PersonelId.Value != 0) ? newExit.PersonelId : null;
            int? newBrand = (newExit.BrandId.HasValue && newExit.BrandId.Value != 0) ? newExit.BrandId : null;
            int? newModel = (newExit.ModelId.HasValue && newExit.ModelId.Value != 0) ? newExit.ModelId : null;

            await _materialInventoryService.AddOrUpdateInventoryAsync(
                oldExit.MaterialId,
                oldLocation,
                oldPerson,
                oldExit.Quantity,
                "MaterialExit güncellendi (eski miktar stokta geri eklendi)",
                newExit.Olusturan,
                oldBrand,
                oldModel,
                cancellationToken);

            await _materialInventoryService.AddOrUpdateInventoryAsync(
                newExit.MaterialId,
                newLocation,
                newPerson,
                -newExit.Quantity,
                "MaterialExit güncellendi (yeni miktar stoktan çıkarıldı)",
                newExit.Olusturan,
                newBrand,
                newModel,
                cancellationToken);

            string operation = EnumHelper.GetEnumDescription((MaterialExitType)newExit.ExitType.Value);
            await _materialMovementService.AddAsync(new MaterialMovement
            {
                MaterialId = newExit.MaterialId,
                Quantity = -newExit.Quantity,
                MaterialUnitId = newExit.MaterialUnitId,
                FromLocationId = newExit.LocationId,
                ToPersonId = newExit.PersonelId,
                MovementDate = newExit.ExitDate,
                MovementType = "Düzeltme",
                Operation = $"Çıkış nedeni: {operation}",
                Aciklama = "MaterialExit güncellendi.",
                Olusturan = newExit.Olusturan,
                OlusturmaTarihi = DateTime.Now,
                BrandId = newBrand,
                ModelId = newModel
            }, cancellationToken);

            await _unitOfWork.MaterialExit.UpdateAsync(newExit);
            await _unitOfWork.MaterialExit.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(MaterialExit exit, CancellationToken cancellationToken = default)
        {
            int? location = exit.LocationId != 0 ? exit.LocationId : null;
            int? person = (exit.PersonelId.HasValue && exit.PersonelId.Value != 0) ? exit.PersonelId : null;
            int? brand = (exit.BrandId.HasValue && exit.BrandId.Value != 0) ? exit.BrandId : null;
            int? model = (exit.ModelId.HasValue && exit.ModelId.Value != 0) ? exit.ModelId : null;

            await _materialInventoryService.AddOrUpdateInventoryAsync(
                exit.MaterialId,
                location,
                person,
                exit.Quantity,
                "MaterialExit silindi, stok geri eklendi.",
                exit.Olusturan,
                brand,
                model,
                cancellationToken);

            await _materialMovementService.AddAsync(new MaterialMovement
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
                Olusturan = exit.Olusturan,
                OlusturmaTarihi = DateTime.Now,
                BrandId = brand,
                ModelId = model
            }, cancellationToken);

            _unitOfWork.MaterialExit.Remove(exit);
            await _unitOfWork.MaterialExit.SaveChangesAsync(cancellationToken);
        }

        public Task<bool> AnyAsync(Expression<Func<MaterialExit, bool>> predicate)
        {
            return _unitOfWork.MaterialExit.AnyAsync(predicate);
        }
    }
}