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

        public async Task AddAsync(MaterialTransfer transfer, CancellationToken cancellationToken = default)
        {
            // 1. MaterialTransfer kaydını ekle
            await _unitOfWork.MaterialTransfer.AddAsync(transfer, cancellationToken);
            await _unitOfWork.MaterialTransfer.SaveChangesAsync(cancellationToken);

            // 2. MaterialInventory - kaynak lokasyondan miktarı düş
            await _materialInventoryService.AddOrUpdateInventoryAsync(
                transfer.MaterialId,
                transfer.FromLocationId,
                transfer.MaterialUnitId,
                -transfer.Quantity,
                "MaterialTransfer: Kaynak stoktan düşüldü.",
                transfer.Olusturan,
                cancellationToken);

            // 3. MaterialInventory - hedef lokasyona miktarı ekle
            await _materialInventoryService.AddOrUpdateInventoryAsync(
                transfer.MaterialId,
                transfer.ToLocationId,
                transfer.MaterialUnitId,
                transfer.Quantity,
                "MaterialTransfer: Hedef stoğa eklendi.",
                transfer.Olusturan,
                cancellationToken);

            // 4. MaterialMovement logu ekle
            var movement = new MaterialMovement
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
                Aciklama = "MaterialTransfer işlemi.",
                Olusturan = transfer.Olusturan,
                OlusturmaTarihi = DateTime.Now
            };
            await _materialMovementService.AddAsync(movement, cancellationToken);
        }
        public async Task UpdateAsync(MaterialTransfer yeniTransfer, CancellationToken cancellationToken = default)
        {
            // Eski kaydı çek
            var eskiTransfer = await GetByIdAsync(yeniTransfer.Id, cancellationToken);
            if (eskiTransfer == null) throw new Exception("Kayıt bulunamadı!");

            // Eski transferi geri al: kaynak stoğa eski miktarı ekle, hedef stoktan eski miktarı çıkar
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

            // Yeni transferi uygula: kaynak stoğa yeni miktarı düş, hedef stoğa yeni miktarı ekle
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

            // MaterialMovement logu ekle
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

            // Kayıt güncelle
            await _unitOfWork.MaterialTransfer.UpdateAsync(yeniTransfer);
            await _unitOfWork.MaterialTransfer.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(MaterialTransfer transfer, CancellationToken cancellationToken = default)
        {
            // Kaynak stoğa transfer edilen miktarı geri ekle
            await _materialInventoryService.AddOrUpdateInventoryAsync(
                transfer.MaterialId,
                transfer.FromLocationId,
                transfer.MaterialUnitId,
                transfer.Quantity,
                "MaterialTransfer silindi, kaynak stoğa geri eklendi.",
                transfer.Olusturan,
                cancellationToken);

            // Hedef stoktan transfer edilen miktarı düş
            await _materialInventoryService.AddOrUpdateInventoryAsync(
                transfer.MaterialId,
                transfer.ToLocationId,
                transfer.MaterialUnitId,
                -transfer.Quantity,
                "MaterialTransfer silindi, hedef stoğundan çıkarıldı.",
                transfer.Olusturan,
                cancellationToken);

            // MaterialMovement logu ekle
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

            // Kayıt sil
            _unitOfWork.MaterialTransfer.Remove(transfer);
            await _unitOfWork.MaterialTransfer.SaveChangesAsync(cancellationToken);
        }
    }
}