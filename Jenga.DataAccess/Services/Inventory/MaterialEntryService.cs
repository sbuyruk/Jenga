using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.Inventory;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.Inventory
{
    public class MaterialEntryService : IMaterialEntryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMaterialInventoryService _materialInventoryService;
        private readonly IMaterialMovementService _materialMovementService;

        public MaterialEntryService(
            IUnitOfWork unitOfWork,
            IMaterialInventoryService materialInventoryService,
            IMaterialMovementService materialMovementService)
        {
            _unitOfWork = unitOfWork;
            _materialInventoryService = materialInventoryService;
            _materialMovementService = materialMovementService;
        }

        public async Task<List<MaterialEntry>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _unitOfWork.MaterialEntry.GetAllAsync(cancellationToken);

        public async Task<MaterialEntry?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => await _unitOfWork.MaterialEntry.GetByIdAsync(id, cancellationToken);

        public async Task<bool> AddAsync(MaterialEntry entry, string? modifiedBy, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.MaterialEntry.AddAsync(entry, cancellationToken);
            await _unitOfWork.MaterialEntry.SaveChangesAsync(cancellationToken);

            // Fetch the material to get its unit
            var material = await _unitOfWork.Material.GetByIdAsync(entry.MaterialId, cancellationToken);
            if (material == null) throw new Exception("Malzeme bulunamadı!");

            // MaterialInventory güncellemesi
            await _materialInventoryService.AddOrUpdateInventoryAsync(
                entry.MaterialId,
                entry.LocationId,
                material.MaterialUnitId,
                entry.Quantity,
                "Malzeme girişi sonrası stok güncellemesi",
                modifiedBy,
                cancellationToken
            );

            // MaterialMovement logu ekle
            await _materialMovementService.AddMovementForEntryAsync(
                entry, "Giriş", "MaterialEntry eklendi", modifiedBy, cancellationToken
            );

            return true;
        }

        public async Task<bool> UpdateMaterialEntryAndInventoryAsync(MaterialEntry entry, string? currentUserName, CancellationToken cancellationToken = default)
        {
            var eskiEntry = await GetByIdAsync(entry.Id, cancellationToken);

            bool miktarDegisti = entry.Quantity != eskiEntry.Quantity;
            bool malzemeDegisti = entry.MaterialId != eskiEntry.MaterialId;
            bool lokasyonDegisti = entry.LocationId != eskiEntry.LocationId;
            bool birimDegisti = entry.MaterialUnitId != eskiEntry.MaterialUnitId;
            currentUserName ??= Environment.UserName;

            if (miktarDegisti && !malzemeDegisti && !lokasyonDegisti && !birimDegisti)
            {
                int fark = entry.Quantity - eskiEntry.Quantity;
                await _materialInventoryService.AddOrUpdateInventoryAsync(
                    entry.MaterialId, entry.LocationId, entry.MaterialUnitId, fark,
                    "Kayıt güncellemesi (miktar değişikliği)",
                    currentUserName, cancellationToken);
            }
            else if (malzemeDegisti || lokasyonDegisti || birimDegisti)
            {
                await _materialInventoryService.AddOrUpdateInventoryAsync(
                    eskiEntry.MaterialId, eskiEntry.LocationId, eskiEntry.MaterialUnitId, -eskiEntry.Quantity,
                    "Kayıt güncellemesi (eski stoktan düş)",
                    currentUserName, cancellationToken);
                await _materialInventoryService.AddOrUpdateInventoryAsync(
                    entry.MaterialId, entry.LocationId, entry.MaterialUnitId, entry.Quantity,
                    "Kayıt güncellemesi (yeni stoğa ekle)",
                    currentUserName, cancellationToken);
            }

            await UpdateAsync(entry, cancellationToken);

            // MaterialMovement logu ekle
            string hareketTipi = (miktarDegisti && !malzemeDegisti && !lokasyonDegisti && !birimDegisti) ? "Düzeltme" : "Transfer";
            await _materialMovementService.AddMovementForEntryAsync(
                entry, hareketTipi, "MaterialEntry güncellendi", currentUserName, cancellationToken
            );

            return true;
        }

        public async Task<bool> UpdateAsync(MaterialEntry entry, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.MaterialEntry.UpdateAsync(entry);
            await _unitOfWork.MaterialEntry.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAsync(MaterialEntry entry, CancellationToken cancellationToken = default)
        {
            _unitOfWork.MaterialEntry.Remove(entry);
            await _unitOfWork.MaterialEntry.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteMaterialEntryAndUpdateInventoryAsync(MaterialEntry silinecekEntry, string? currentUserName, CancellationToken cancellationToken = default)
        {
            currentUserName ??= Environment.UserName;
            if (silinecekEntry == null)
                return false;

            // 1. Stoktan çıkar
            await _materialInventoryService.AddOrUpdateInventoryAsync(
                silinecekEntry.MaterialId,
                silinecekEntry.LocationId,
                silinecekEntry.MaterialUnitId,
                -silinecekEntry.Quantity,
                "MaterialEntry silindi, stoktan çıkarıldı",
                currentUserName,
                cancellationToken
            );

            // 2. (İsteğe bağlı) Stok 0 veya altı ise sil
            var stokKaydi = await _materialInventoryService.GetByMaterialLocationAsync(
                silinecekEntry.MaterialId,
                silinecekEntry.LocationId,
                silinecekEntry.MaterialUnitId,
                cancellationToken
            );
            if (stokKaydi != null && stokKaydi.Quantity <= 0)
            {
                await _materialInventoryService.DeleteAsync(stokKaydi, cancellationToken);
            }

            // 3. MaterialEntry kaydını sil
            await DeleteAsync(silinecekEntry, cancellationToken);

            // MaterialMovement logu ekle
            await _materialMovementService.AddMovementForEntryAsync(
                silinecekEntry, "Silme", "MaterialEntry silindi", currentUserName, cancellationToken
            );

            return true;
        }

        public Task<bool> AnyAsync(Expression<Func<MaterialEntry, bool>> predicate)
        {
            return _unitOfWork.MaterialEntry.AnyAsync(predicate);
        }
    }
}