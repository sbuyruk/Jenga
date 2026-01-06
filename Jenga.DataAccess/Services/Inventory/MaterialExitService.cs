using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.Enums;
using Jenga.Models.Inventory;
using Jenga.Utility.Helpers;
using System.Linq.Expressions;
using System;

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

            // Normalize 0 -> null for optional ids
            int? actualLocation = exit.LocationId != 0 ? exit.LocationId : null;
            int? actualPerson = (exit.PersonelId.HasValue && exit.PersonelId.Value != 0) ? exit.PersonelId : null;
            int? actualBrand = (exit.BrandId.HasValue && exit.BrandId.Value != 0) ? exit.BrandId : null;
            int? actualModel = (exit.ModelId.HasValue && exit.ModelId.Value != 0) ? exit.ModelId : null;

            // 2. MaterialInventory'den miktarı düş (include brand/model)
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

            // 3. MaterialMovement logu ekle (include brand/model)
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
        }
        public async Task UpdateAsync(MaterialExit newExit, CancellationToken cancellationToken = default)
        {
            // Eski kaydı çek
            var oldExit = await GetByIdAsync(newExit.Id, cancellationToken);
            if (oldExit == null) throw new Exception("Kayıt bulunamadı!");

            // Normalize ids (0->null)
            int? oldLocation = oldExit.LocationId != 0 ? oldExit.LocationId : null;
            int? oldPerson = (oldExit.PersonelId.HasValue && oldExit.PersonelId.Value != 0) ? oldExit.PersonelId : null;
            int? oldBrand = (oldExit.BrandId.HasValue && oldExit.BrandId.Value != 0) ? oldExit.BrandId : null;
            int? oldModel = (oldExit.ModelId.HasValue && oldExit.ModelId.Value != 0) ? oldExit.ModelId : null;

            int? newLocation = newExit.LocationId != 0 ? newExit.LocationId : null;
            int? newPerson = (newExit.PersonelId.HasValue && newExit.PersonelId.Value != 0) ? newExit.PersonelId : null;
            int? newBrand = (newExit.BrandId.HasValue && newExit.BrandId.Value != 0) ? newExit.BrandId : null;
            int? newModel = (newExit.ModelId.HasValue && newExit.ModelId.Value != 0) ? newExit.ModelId : null;

            // Eski miktarı envantere geri ekle (include old brand/model)
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

            // Yeni miktarı envanterden düş (include new brand/model)
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

            // MaterialMovement logu ekle (include brand/model)
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

            // Kayıt güncelle
            await _unitOfWork.MaterialExit.UpdateAsync(newExit);
            await _unitOfWork.MaterialExit.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(MaterialExit exit, CancellationToken cancellationToken = default)
        {
            // Normalize ids
            int? location = exit.LocationId != 0 ? exit.LocationId : null;
            int? person = (exit.PersonelId.HasValue && exit.PersonelId.Value != 0) ? exit.PersonelId : null;
            int? brand = (exit.BrandId.HasValue && exit.BrandId.Value != 0) ? exit.BrandId : null;
            int? model = (exit.ModelId.HasValue && exit.ModelId.Value != 0) ? exit.ModelId : null;

            // Envantere miktarı geri ekle (include brand/model)
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

            // MaterialMovement logu ekle (include brand/model)
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