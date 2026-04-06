using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.Inventory;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.Inventory
{
    public class MaterialTransferService : IMaterialTransferService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMaterialInventoryService _materialInventoryService;
        private readonly IMaterialMovementService _materialMovementService;
        private readonly IMaterialAssetService _materialAssetService;
        private readonly IMaterialAssetLogService _materialAssetLogService;

        public MaterialTransferService(
            IUnitOfWork unitOfWork,
            IMaterialInventoryService materialInventoryService,
            IMaterialMovementService materialMovementService,
            IMaterialAssetService materialAssetService,
            IMaterialAssetLogService materialAssetLogService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _materialInventoryService = materialInventoryService ?? throw new ArgumentNullException(nameof(materialInventoryService));
            _materialMovementService = materialMovementService ?? throw new ArgumentNullException(nameof(materialMovementService));
            _materialAssetService = materialAssetService ?? throw new ArgumentNullException(nameof(materialAssetService));
            _materialAssetLogService = materialAssetLogService ?? throw new ArgumentNullException(nameof(materialAssetLogService));
        }

        public async Task<List<MaterialTransfer>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _unitOfWork.MaterialTransfer.GetAllAsync(cancellationToken);

        public async Task<MaterialTransfer?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => await _unitOfWork.MaterialTransfer.GetByIdAsync(id, cancellationToken);

        public Task<bool> AnyAsync(Expression<Func<MaterialTransfer, bool>> predicate)
            => _unitOfWork.MaterialTransfer.AnyAsync(predicate);

        public async Task<bool> AddAsync(MaterialTransfer transfer, string? modifiedBy = null, List<int>? selectedAssetIds = null, CancellationToken cancellationToken = default)
        {
            if (transfer == null) throw new ArgumentNullException(nameof(transfer));

            await _unitOfWork.MaterialTransfer.AddAsync(transfer, cancellationToken);
            await _unitOfWork.MaterialTransfer.SaveChangesAsync(cancellationToken);

            int? actualFromLocation = transfer.FromLocationId != 0 ? transfer.FromLocationId : null;
            int? actualToLocation = transfer.ToLocationId != 0 ? transfer.ToLocationId : null;
            int? actualFromPerson = (transfer.FromPersonId.HasValue && transfer.FromPersonId != 0) ? transfer.FromPersonId : null;
            int? actualToPerson = (transfer.ToPersonId.HasValue && transfer.ToPersonId != 0) ? transfer.ToPersonId : null;
            int? actualBrand = (transfer.BrandId.HasValue && transfer.BrandId != 0) ? transfer.BrandId : null;
            int? actualModel = (transfer.ModelId.HasValue && transfer.ModelId != 0) ? transfer.ModelId : null;

            await _materialInventoryService.AddOrUpdateInventoryAsync(
                transfer.MaterialId,
                actualFromLocation,
                actualFromPerson,
                -transfer.Quantity,
                "MaterialTransfer: Kaynak stoktan düşüldü.",
                modifiedBy,
                actualBrand,
                actualModel,
                cancellationToken);

            await _materialInventoryService.AddOrUpdateInventoryAsync(
                transfer.MaterialId,
                actualToLocation,
                actualToPerson,
                transfer.Quantity,
                "MaterialTransfer: Hedef stoğa eklendi.",
                modifiedBy,
                actualBrand,
                actualModel,
                cancellationToken);

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
            await _materialMovementService.AddAsync(movement, cancellationToken);

            var material = await _unitOfWork.Material.GetByIdAsync(transfer.MaterialId, cancellationToken);
            if (material != null && material.IsAsset)
            {
                await TransferAssetsAsync(
                    transfer.MaterialId,
                    transfer.Quantity,
                    actualFromLocation,
                    actualFromPerson,
                    actualToLocation,
                    actualToPerson,
                    actualBrand,
                    actualModel,
                    modifiedBy,
                    selectedAssetIds,
                    cancellationToken);
            }

            return true;
        }

        private async Task TransferAssetsAsync(
            int materialId,
            int quantity,
            int? fromLocationId,
            int? fromPersonId,
            int? toLocationId,
            int? toPersonId,
            int? brandId,
            int? modelId,
            string? modifiedBy,
            List<int>? selectedAssetIds,
            CancellationToken cancellationToken)
        {
            List<MaterialAsset> assetsToTransfer;

            if (selectedAssetIds != null && selectedAssetIds.Count > 0)
            {
                var allAssets = await _materialAssetService.GetByMaterialIdAsync(materialId, cancellationToken);
                assetsToTransfer = allAssets
                    .Where(a => selectedAssetIds.Contains(a.Id) && a.Status == AssetStatus.Active)
                    .ToList();
            }
            else
            {
                var sourceAssets = await _materialAssetService.GetByMaterialIdAsync(materialId, cancellationToken);
                assetsToTransfer = sourceAssets
                    .Where(a => a.Status == AssetStatus.Active
                        && a.LocationId == fromLocationId
                        && a.PersonelId == fromPersonId
                        && a.BrandId == brandId
                        && a.ModelId == modelId)
                    .Take(quantity)
                    .ToList();
            }

            foreach (var asset in assetsToTransfer)
            {
                var log = new MaterialAssetLog
                {
                    MaterialAssetId = asset.Id,
                    FromPersonelId = asset.PersonelId,
                    ToPersonelId = toPersonId,
                    FromLocationId = asset.LocationId,
                    ToLocationId = toLocationId,
                    TransactionDate = DateTime.Now,
                    TransactionType = "Transfer",
                    Aciklama = $"Transfer #{asset.SerialNumber ?? asset.Id.ToString()}",
                    Olusturan = modifiedBy,
                    OlusturmaTarihi = DateTime.Now
                };
                await _materialAssetLogService.AddAsync(log, cancellationToken);

                asset.LocationId = toLocationId;
                asset.PersonelId = toPersonId;
                asset.Degistiren = modifiedBy;
                asset.DegistirmeTarihi = DateTime.Now;
                await _materialAssetService.UpdateAsync(asset, cancellationToken);
            }
        }

        public async Task<bool> UpdateAsync(MaterialTransfer yeniTransfer, CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(false);
        }

        public async Task<bool> DeleteAsync(MaterialTransfer transfer, CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(false);
        }

        public async Task<bool> UpdateMaterialTransferAndInventoryAsync(MaterialTransfer transfer, string? currentUserName, CancellationToken cancellationToken = default)
        {
            return await UpdateAsync(transfer, cancellationToken);
        }

        public async Task<bool> DeleteMaterialTransferAndUpdateInventoryAsync(MaterialTransfer transfer, string? currentUserName, CancellationToken cancellationToken = default)
        {
            return await DeleteAsync(transfer, cancellationToken);
        }
    }
}