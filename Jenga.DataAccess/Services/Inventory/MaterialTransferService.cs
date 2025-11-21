using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.Inventory;
using Jenga.DataAccess.Services.IKYS;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.Inventory
{
    public class MaterialTransferService : IMaterialTransferService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMaterialInventoryService _materialInventoryService;
        private readonly IMaterialMovementService _materialMovementService;

        public MaterialTransferService(
            IUnitOfWork unitOfWork,
            IMaterialInventoryService materialInventoryService,
            IMaterialMovementService materialMovementService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _materialInventoryService = materialInventoryService ?? throw new ArgumentNullException(nameof(materialInventoryService));
            _materialMovementService = materialMovementService ?? throw new ArgumentNullException(nameof(materialMovementService));
        }

        public async Task<List<MaterialTransfer>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _unitOfWork.MaterialTransfer.GetAllAsync(cancellationToken);

        public async Task<MaterialTransfer?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => await _unitOfWork.MaterialTransfer.GetByIdAsync(id, cancellationToken);

        public Task<bool> AnyAsync(Expression<Func<MaterialTransfer, bool>> predicate)
            => _unitOfWork.MaterialTransfer.AnyAsync(predicate);

        public async Task<bool> AddAsync(MaterialTransfer transfer, string? modifiedBy = null, CancellationToken cancellationToken = default)
        {
            if (transfer == null) throw new ArgumentNullException(nameof(transfer));

            // persist transfer record
            await _unitOfWork.MaterialTransfer.AddAsync(transfer, cancellationToken);
            await _unitOfWork.MaterialTransfer.SaveChangesAsync(cancellationToken);

            // Resolve physical locations to use for inventory operations (nullable)
            int? actualFromLocation = transfer.FromLocationId != 0 ? transfer.FromLocationId : null;
            int? actualToLocation = transfer.ToLocationId != 0 ? transfer.ToLocationId : null;

            // If FromPersonId provided: prefer a person-specific inventory row (PersonelId) or fallback to person's primary location
            if (transfer.FromPersonId.HasValue)
            {
                var personInv = (await _unitOfWork.MaterialInventory.GetAllAsync(cancellationToken))
                    .FirstOrDefault(mi => mi.MaterialId == transfer.MaterialId && mi.PersonelId == transfer.FromPersonId);
                if (personInv != null)
                {
                    // Use person inventory row's LocationId (could be null)
                    actualFromLocation = personInv.LocationId;
                }
            }

            // Perform inventory updates using triples (materialId, locationId?, personelId?)
            // Decrement source (note: pass the FromPersonId so person-specific rows are preferred)
            await _materialInventoryService.AddOrUpdateInventoryAsync(
                transfer.MaterialId,
                actualFromLocation,
                transfer.FromPersonId,
                -transfer.Quantity,
                "MaterialTransfer: Kaynak stoktan düşüldü.",
                modifiedBy,
                cancellationToken);

            // Increment target
            await _materialInventoryService.AddOrUpdateInventoryAsync(
                transfer.MaterialId,
                actualToLocation,
                transfer.ToPersonId,
                transfer.Quantity,
                "MaterialTransfer: Hedef stoğa eklendi.",
                modifiedBy,
                cancellationToken);

            // Add movement/audit
            await _materialMovementService.AddAsync(new MaterialMovement
            {
                MaterialId = transfer.MaterialId,
                Quantity = transfer.Quantity,
                MaterialUnitId = transfer.MaterialUnitId,
                FromLocationId = actualFromLocation ?? transfer.FromLocationId,
                ToLocationId = actualToLocation ?? transfer.ToLocationId,
                FromPersonId = transfer.FromPersonId,
                ToPersonId = transfer.ToPersonId,
                MovementDate = transfer.TransferDate,
                MovementType = "Transfer",
                Aciklama = transfer.Aciklama ?? "MaterialTransfer işlemi",
                Olusturan = modifiedBy,
                OlusturmaTarihi = DateTime.Now
            }, cancellationToken);

            return true;
        }

        public async Task<bool> UpdateAsync(MaterialTransfer yeniTransfer, CancellationToken cancellationToken = default)
        {
            if (yeniTransfer == null) throw new ArgumentNullException(nameof(yeniTransfer));

            var eskiTransfer = await GetByIdAsync(yeniTransfer.Id, cancellationToken);
            if (eskiTransfer == null) throw new Exception("Kayıt bulunamadı!");

            // Resolve physical locations/person-inv for old transfer
            int? eskiFromLoc = eskiTransfer.FromLocationId != 0 ? eskiTransfer.FromLocationId : null;
            int? eskiToLoc = eskiTransfer.ToLocationId != 0 ? eskiTransfer.ToLocationId : null;

            if (eskiTransfer.FromPersonId.HasValue)
            {
                var personInv = (await _unitOfWork.MaterialInventory.GetAllAsync(cancellationToken))
                    .FirstOrDefault(mi => mi.MaterialId == eskiTransfer.MaterialId && mi.PersonelId == eskiTransfer.FromPersonId);

            }

            // Resolve physical locations/person-inv for new transfer
            int? yeniFromLoc = yeniTransfer.FromLocationId != 0 ? yeniTransfer.FromLocationId : null;
            int? yeniToLoc = yeniTransfer.ToLocationId != 0 ? yeniTransfer.ToLocationId : null;

            if (yeniTransfer.FromPersonId.HasValue)
            {
                var personInv = (await _unitOfWork.MaterialInventory.GetAllAsync(cancellationToken))
                    .FirstOrDefault(mi => mi.MaterialId == yeniTransfer.MaterialId && mi.PersonelId == yeniTransfer.FromPersonId);
            }
            // Rollback old inventory effects (credit old source, debit old target) where physical rows identified
            if (eskiFromLoc.HasValue || eskiTransfer.FromPersonId.HasValue)
            {
                await _materialInventoryService.AddOrUpdateInventoryAsync(
                    eskiTransfer.MaterialId,
                    eskiFromLoc,
                    eskiTransfer.FromPersonId,
                    eskiTransfer.Quantity,
                    "MaterialTransfer güncellendi (eski transfer geri alındı - kaynak geri eklendi)",
                    yeniTransfer.Olusturan,
                    cancellationToken);
            }

            if (eskiToLoc.HasValue || eskiTransfer.ToPersonId.HasValue)
            {
                await _materialInventoryService.AddOrUpdateInventoryAsync(
                    eskiTransfer.MaterialId,
                    eskiToLoc,
                    eskiTransfer.ToPersonId,
                    -eskiTransfer.Quantity,
                    "MaterialTransfer güncellendi (eski transfer hedef stoğundan düşüldü)",
                    yeniTransfer.Olusturan,
                    cancellationToken);
            }

            // Apply new transfer inventory effects
            if (yeniFromLoc.HasValue || yeniTransfer.FromPersonId.HasValue)
            {
                await _materialInventoryService.AddOrUpdateInventoryAsync(
                    yeniTransfer.MaterialId,
                    yeniFromLoc,
                    yeniTransfer.FromPersonId,
                    -yeniTransfer.Quantity,
                    "MaterialTransfer güncellendi (yeni transfer kaynak stoğundan düşüldü)",
                    yeniTransfer.Olusturan,
                    cancellationToken);
            }

            if (yeniToLoc.HasValue || yeniTransfer.ToPersonId.HasValue)
            {
                await _materialInventoryService.AddOrUpdateInventoryAsync(
                    yeniTransfer.MaterialId,
                    yeniToLoc,
                    yeniTransfer.ToPersonId,
                    yeniTransfer.Quantity,
                    "MaterialTransfer güncellendi (yeni transfer hedef stoğuna eklendi)",
                    yeniTransfer.Olusturan,
                    cancellationToken);
            }

            // Log correction movement
            await _materialMovementService.AddAsync(new MaterialMovement
            {
                MaterialId = yeniTransfer.MaterialId,
                Quantity = yeniTransfer.Quantity,
                MaterialUnitId = yeniTransfer.MaterialUnitId,
                FromLocationId = yeniFromLoc ?? yeniTransfer.FromLocationId,
                ToLocationId = yeniToLoc ?? yeniTransfer.ToLocationId,
                FromPersonId = yeniTransfer.FromPersonId,
                ToPersonId = yeniTransfer.ToPersonId,
                MovementDate = yeniTransfer.TransferDate,
                MovementType = "Transfer-Düzeltme",
                Aciklama = "MaterialTransfer güncellendi.",
                Olusturan = yeniTransfer.Olusturan,
                OlusturmaTarihi = DateTime.Now
            }, cancellationToken);

            // update transfer record
            await _unitOfWork.MaterialTransfer.UpdateAsync(yeniTransfer);
            await _unitOfWork.MaterialTransfer.SaveChangesAsync(cancellationToken);

            return true;
        }

        public async Task<bool> DeleteAsync(MaterialTransfer transfer, CancellationToken cancellationToken = default)
        {
            if (transfer == null) throw new ArgumentNullException(nameof(transfer));

            // Resolve locations as in Add/Update
            int? fromLoc = transfer.FromLocationId != 0 ? transfer.FromLocationId : null;
            int? toLoc = transfer.ToLocationId != 0 ? transfer.ToLocationId : null;

            if (transfer.FromPersonId.HasValue)
            {
                var personInv = (await _unitOfWork.MaterialInventory.GetAllAsync(cancellationToken))
                    .FirstOrDefault(mi => mi.MaterialId == transfer.MaterialId && mi.PersonelId == transfer.FromPersonId);
            }

            // Revert inventory effects where applicable
            if (fromLoc.HasValue || transfer.FromPersonId.HasValue)
            {
                await _materialInventoryService.AddOrUpdateInventoryAsync(
                    transfer.MaterialId,
                    fromLoc,
                    transfer.FromPersonId,
                    transfer.Quantity,
                    "MaterialTransfer silindi, kaynak stoğa geri eklendi.",
                    transfer.Olusturan,
                    cancellationToken);
            }

            if (toLoc.HasValue || transfer.ToPersonId.HasValue)
            {
                await _materialInventoryService.AddOrUpdateInventoryAsync(
                    transfer.MaterialId,
                    toLoc,
                    transfer.ToPersonId,
                    -transfer.Quantity,
                    "MaterialTransfer silindi, hedef stoğundan çıkarıldı.",
                    transfer.Olusturan,
                    cancellationToken);
            }

            // Log reverse movement
            await _materialMovementService.AddAsync(new MaterialMovement
            {
                MaterialId = transfer.MaterialId,
                Quantity = -transfer.Quantity,
                MaterialUnitId = transfer.MaterialUnitId,
                FromLocationId = fromLoc ?? transfer.FromLocationId,
                ToLocationId = toLoc ?? transfer.ToLocationId,
                FromPersonId = transfer.FromPersonId,
                ToPersonId = transfer.ToPersonId,
                MovementDate = DateTime.Now,
                MovementType = "Transfer-Silme",
                Aciklama = "MaterialTransfer silindi.",
                Olusturan = transfer.Olusturan,
                OlusturmaTarihi = DateTime.Now
            }, cancellationToken);

            _unitOfWork.MaterialTransfer.Remove(transfer);
            await _unitOfWork.MaterialTransfer.SaveChangesAsync(cancellationToken);

            return true;
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