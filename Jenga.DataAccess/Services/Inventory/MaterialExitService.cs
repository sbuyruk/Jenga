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

        public MaterialExitService(
             IUnitOfWork unitOfWork,
             IMaterialInventoryService materialInventoryService,
             IMaterialMovementService materialMovementService)
        {
            _unitOfWork = unitOfWork;
            _materialInventoryService = materialInventoryService;
            _materialMovementService = materialMovementService;
        }
        public async Task<List<MaterialExit>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _unitOfWork.MaterialExit.GetAllAsync(cancellationToken);

        public async Task<MaterialExit?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => await _unitOfWork.MaterialExit.GetByIdAsync(id, cancellationToken);

        public async Task AddAsync(MaterialExit exit, CancellationToken cancellationToken = default)
        {
            // 1. MaterialExit kaydını ekle
            await _unitOfWork.MaterialExit.AddAsync(exit, cancellationToken);
            await _unitOfWork.MaterialExit.SaveChangesAsync(cancellationToken);

            // Get the material to access its unit
            var material = await _unitOfWork.Material.GetByIdAsync(exit.MaterialId, cancellationToken);
            if (material == null) throw new Exception("Malzeme bulunamadı!");

            // 2. MaterialInventory'den miktarı düş
            await _materialInventoryService.AddOrUpdateInventoryAsync(
                exit.MaterialId,
                exit.LocationId,
                material.MaterialUnitId,
                -exit.Quantity,
                $"MaterialExit: {exit.ExitType} işlemi ile stoktan çıkarıldı.",
                exit.Olusturan,
                cancellationToken);

            // 3. MaterialMovement logu ekle
            string aciklama = EnumHelper.GetEnumDescription((MaterialExitType)exit.ExitType.Value);
            var movement = new MaterialMovement
            {
                MaterialId = exit.MaterialId,
                Quantity = -exit.Quantity,
                MaterialUnitId = material.MaterialUnitId,
                FromLocationId = exit.LocationId,
                ToLocationId = null,
                FromPersonId = null,
                ToPersonId = exit.PersonId,
                MovementDate = exit.ExitDate,
                MovementType ="Çıkış",
                Aciklama = $"MaterialExit: {aciklama} işlemi.",
                Olusturan = exit.Olusturan,
                OlusturmaTarihi = DateTime.Now
            };
            await _materialMovementService.AddAsync(movement, cancellationToken);
        }
        public async Task UpdateAsync(MaterialExit yeniExit, CancellationToken cancellationToken = default)
        {
            // Eski kaydı çek
            var eskiExit = await GetByIdAsync(yeniExit.Id, cancellationToken);
            if (eskiExit == null) throw new Exception("Kayıt bulunamadı!");

            // Eski miktarı envantere geri ekle
            await _materialInventoryService.AddOrUpdateInventoryAsync(
                eskiExit.MaterialId,
                eskiExit.LocationId,
                eskiExit.MaterialUnitId,
                eskiExit.Quantity,
                "MaterialExit güncellendi (eski miktar stokta geri eklendi)",
                yeniExit.Olusturan,
                cancellationToken);

            // Yeni miktarı envanterden düş
            await _materialInventoryService.AddOrUpdateInventoryAsync(
                yeniExit.MaterialId,
                yeniExit.LocationId,
                yeniExit.MaterialUnitId,
                -yeniExit.Quantity,
                "MaterialExit güncellendi (yeni miktar stoktan çıkarıldı)",
                yeniExit.Olusturan,
                cancellationToken);

            // MaterialMovement logu ekle
            await _materialMovementService.AddAsync(new MaterialMovement
            {
                MaterialId = yeniExit.MaterialId,
                Quantity = -yeniExit.Quantity,
                MaterialUnitId = yeniExit.MaterialUnitId,
                FromLocationId = yeniExit.LocationId,
                ToPersonId = yeniExit.PersonId,
                MovementDate = yeniExit.ExitDate,
                MovementType = "Düzeltme",
                Aciklama = "MaterialExit güncellendi.",
                Olusturan = yeniExit.Olusturan,
                OlusturmaTarihi = DateTime.Now
            }, cancellationToken);

            // Kayıt güncelle
            await _unitOfWork.MaterialExit.UpdateAsync(yeniExit);
            await _unitOfWork.MaterialExit.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(MaterialExit exit, CancellationToken cancellationToken = default)
        {
            // Envantere miktarı geri ekle
            await _materialInventoryService.AddOrUpdateInventoryAsync(
                exit.MaterialId,
                exit.LocationId,
                exit.MaterialUnitId,
                exit.Quantity,
                "MaterialExit silindi, stok geri eklendi.",
                exit.Olusturan,
                cancellationToken);

            // MaterialMovement logu ekle
            await _materialMovementService.AddAsync(new MaterialMovement
            {
                MaterialId = exit.MaterialId,
                Quantity = exit.Quantity,
                MaterialUnitId = exit.MaterialUnitId,
                FromLocationId = exit.LocationId,
                ToPersonId = exit.PersonId,
                MovementDate = DateTime.Now,
                MovementType = "Silme",
                Aciklama = "MaterialExit silindi.",
                Olusturan = exit.Olusturan,
                OlusturmaTarihi = DateTime.Now
            }, cancellationToken);

            // Kayıt sil
            _unitOfWork.MaterialExit.Remove(exit);
            await _unitOfWork.MaterialExit.SaveChangesAsync(cancellationToken);
        }
        public Task<bool> AnyAsync(Expression<Func<MaterialExit, bool>> predicate)
        {
            return _unitOfWork.MaterialExit.AnyAsync(predicate);
        }

    }
}