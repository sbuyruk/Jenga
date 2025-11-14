using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.Inventory;

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
            _unitOfWork = unitOfWork;
            _materialInventoryService = materialInventoryService;
            _materialMovementService = materialMovementService;
        }

        public async Task<List<MaterialTransfer>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _unitOfWork.MaterialTransfer.GetAllAsync(cancellationToken);

        public async Task<MaterialTransfer?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => await _unitOfWork.MaterialTransfer.GetByIdAsync(id, cancellationToken);

        public async Task<bool> AddAsync(MaterialTransfer transfer, string? modifiedBy, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.MaterialTransfer.AddAsync(transfer, cancellationToken);
            await _unitOfWork.MaterialTransfer.SaveChangesAsync(cancellationToken);

            await _materialInventoryService.AddOrUpdateInventoryAsync(
                transfer.MaterialId,
                transfer.FromLocationId,
                transfer.MaterialUnitId,
                -transfer.Quantity,
                "MaterialTransfer: Kaynak stoktan düşüldü.",
                modifiedBy,
                cancellationToken);

            await _materialInventoryService.AddOrUpdateInventoryAsync(
                transfer.MaterialId,
                transfer.ToLocationId,
                transfer.MaterialUnitId,
                transfer.Quantity,
                "MaterialTransfer: Hedef stoğa eklendi.",
                modifiedBy,
                cancellationToken);

            // Add movement log
            await _materialMovementService.AddAsync(new MaterialMovement
            {
                MaterialId = transfer.MaterialId,
                Quantity = transfer.Quantity,
                MaterialUnitId = transfer.MaterialUnitId,
                FromLocationId = transfer.FromLocationId,
                ToLocationId = transfer.ToLocationId,
                FromPersonId = transfer.FromPersonId,
                ToPersonId = transfer.ToPersonId,
                MovementDate = transfer.TransferDate,
                MovementType = "Transfer",
                Aciklama = "MaterialTransfer işlemi",
                Olusturan = modifiedBy,
                OlusturmaTarihi = DateTime.Now
            }, cancellationToken);

            return true;
        }

        public async Task<bool> UpdateAsync(MaterialTransfer yeniTransfer, CancellationToken cancellationToken = default)
        {
            var eskiTransfer = await GetByIdAsync(yeniTransfer.Id, cancellationToken);
            if (eskiTransfer == null) throw new Exception("Kayıt bulunamadı!");

            // rollback old transfer
            await _materialInventoryService.AddOrUpdateInventoryAsync(
                eskiTransfer.MaterialId,
                eskiTransfer.FromLocationId,
                eskiTransfer.MaterialUnitId,
                eskiTransfer.Quantity,
                "MaterialTransfer güncellendi (eski transfer geri alındı)",
                yeniTransfer.Olusturan,
                cancellationToken);

            await _materialInventoryService.AddOrUpdateInventoryAsync(
                eskiTransfer.MaterialId,
                eskiTransfer.ToLocationId,
                eskiTransfer.MaterialUnitId,
                -eskiTransfer.Quantity,
                "MaterialTransfer güncellendi (eski transfer hedef stoğundan düşüldü)",
                yeniTransfer.Olusturan,
                cancellationToken);

            // apply new transfer
            await _materialInventoryService.AddOrUpdateInventoryAsync(
                yeniTransfer.MaterialId,
                yeniTransfer.FromLocationId,
                yeniTransfer.MaterialUnitId,
                -yeniTransfer.Quantity,
                "MaterialTransfer güncellendi (yeni transfer kaynak stoğundan düşüldü)",
                yeniTransfer.Olusturan,
                cancellationToken);

            await _materialInventoryService.AddOrUpdateInventoryAsync(
                yeniTransfer.MaterialId,
                yeniTransfer.ToLocationId,
                yeniTransfer.MaterialUnitId,
                yeniTransfer.Quantity,
                "MaterialTransfer güncellendi (yeni transfer hedef stoğuna eklendi)",
                yeniTransfer.Olusturan,
                cancellationToken);

            await _materialMovementService.AddAsync(new MaterialMovement
            {
                MaterialId = yeniTransfer.MaterialId,
                Quantity = yeniTransfer.Quantity,
                MaterialUnitId = yeniTransfer.MaterialUnitId,
                FromLocationId = yeniTransfer.FromLocationId,
                ToLocationId = yeniTransfer.ToLocationId,
                FromPersonId = yeniTransfer.FromPersonId,
                ToPersonId = yeniTransfer.ToPersonId,
                MovementDate = yeniTransfer.TransferDate,
                MovementType = "Transfer-Düzeltme",
                Aciklama = "MaterialTransfer güncellendi.",
                Olusturan = yeniTransfer.Olusturan,
                OlusturmaTarihi = DateTime.Now
            }, cancellationToken);

            await _unitOfWork.MaterialTransfer.UpdateAsync(yeniTransfer);
            await _unitOfWork.MaterialTransfer.SaveChangesAsync(cancellationToken);

            return true;
        }

        public async Task<bool> DeleteAsync(MaterialTransfer transfer, CancellationToken cancellationToken = default)
        {
            // revert: add back to source and subtract from destination
            await _materialInventoryService.AddOrUpdateInventoryAsync(
                transfer.MaterialId,
                transfer.FromLocationId,
                transfer.MaterialUnitId,
                transfer.Quantity,
                "MaterialTransfer silindi, kaynak stoğa geri eklendi.",
                transfer.Olusturan,
                cancellationToken);

            await _materialInventoryService.AddOrUpdateInventoryAsync(
                transfer.MaterialId,
                transfer.ToLocationId,
                transfer.MaterialUnitId,
                -transfer.Quantity,
                "MaterialTransfer silindi, hedef stoğundan çıkarıldı.",
                transfer.Olusturan,
                cancellationToken);

            await _materialMovementService.AddAsync(new MaterialMovement
            {
                MaterialId = transfer.MaterialId,
                Quantity = -transfer.Quantity,
                MaterialUnitId = transfer.MaterialUnitId,
                FromLocationId = transfer.FromLocationId,
                ToLocationId = transfer.ToLocationId,
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

        public Task<bool> AnyAsync(System.Linq.Expressions.Expression<Func<MaterialTransfer, bool>> predicate)
        {
            return _unitOfWork.MaterialTransfer.AnyAsync(predicate);
        }

        public async Task<bool> UpdateMaterialTransferAndInventoryAsync(MaterialTransfer transfer, string? currentUserName, CancellationToken cancellationToken = default)
        {
            // reuse UpdateAsync
            return await UpdateAsync(transfer, cancellationToken);
        }

        public async Task<bool> DeleteMaterialTransferAndUpdateInventoryAsync(MaterialTransfer transfer, string? currentUserName, CancellationToken cancellationToken = default)
        {
            return await DeleteAsync(transfer, cancellationToken);
        }
    }
}