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

            // Malzeme bilgisini sadece loglama veya kontrol için çekiyoruz, zorunlu değilse kaldırılabilir.
            // var material = await _unitOfWork.Material.GetByIdAsync(entry.MaterialId, cancellationToken);

            // DÜZELTME: 0 gelen ID'leri NULL'a çeviriyoruz.
            int? actualLocationId = entry.LocationId != 0 ? entry.LocationId : null;
            // PersonelId zaten int? olabilir ama modelde int ise kontrol gerekir.
            // Genelde PersonelId nullable int ise sorun yok, int ise ve 0 ise null yapılmalı.
            // Burada entry.PersonelId özelliğinin int? olduğunu varsayarak:
            int? actualPersonelId = (entry.PersonelId.HasValue && entry.PersonelId.Value != 0) ? entry.PersonelId : null;

            // MaterialInventory güncellemesi
            await _materialInventoryService.AddOrUpdateInventoryAsync(
                entry.MaterialId,
                actualLocationId,
                actualPersonelId,
                entry.Quantity,
                "Malzeme girişi sonrası stok güncellemesi",
                modifiedBy,
                cancellationToken
            );

            await _materialMovementService.AddMovementForEntryAsync(
                entry, "Giriş", "MaterialEntry eklendi", modifiedBy, cancellationToken
            );

            return true;
        }

        public async Task<bool> UpdateMaterialEntryAndInventoryAsync(MaterialEntry entry, string? currentUserName, CancellationToken cancellationToken = default)
        {
            var eskiEntry = await GetByIdAsync(entry.Id, cancellationToken);
            if (eskiEntry == null) throw new Exception("Eski kayıt bulunamadı.");

            bool miktarDegisti = entry.Quantity != eskiEntry.Quantity;
            bool malzemeDegisti = entry.MaterialId != eskiEntry.MaterialId;
            bool lokasyonDegisti = entry.LocationId != eskiEntry.LocationId;
            bool birimDegisti = entry.MaterialUnitId != eskiEntry.MaterialUnitId;
            bool personelDegisti = entry.PersonelId != eskiEntry.PersonelId;

            currentUserName ??= Environment.UserName;

            // ID Düzeltmeleri
            int? yeniLoc = entry.LocationId != 0 ? entry.LocationId : null;
            int? yeniPers = (entry.PersonelId.HasValue && entry.PersonelId.Value != 0) ? entry.PersonelId : null;

            int? eskiLoc = eskiEntry.LocationId != 0 ? eskiEntry.LocationId : null;
            int? eskiPers = (eskiEntry.PersonelId.HasValue && eskiEntry.PersonelId.Value != 0) ? eskiEntry.PersonelId : null;

            if (miktarDegisti && !malzemeDegisti && !lokasyonDegisti && !birimDegisti && !personelDegisti)
            {
                int fark = entry.Quantity - eskiEntry.Quantity;
                await _materialInventoryService.AddOrUpdateInventoryAsync(
                    entry.MaterialId,
                    yeniLoc,
                    yeniPers,
                    fark,
                    "Kayıt güncellemesi (miktar değişikliği)",
                    currentUserName, cancellationToken);
            }
            else if (malzemeDegisti || lokasyonDegisti || birimDegisti || personelDegisti)
            {
                // Eski kaydı geri al (stoktan düş)
                await _materialInventoryService.AddOrUpdateInventoryAsync(
                    eskiEntry.MaterialId,
                    eskiLoc,
                    eskiPers,
                    -eskiEntry.Quantity,
                    "Kayıt güncellemesi (eski stoktan düş)",
                    currentUserName, cancellationToken);

                // Yeni kaydı ekle
                await _materialInventoryService.AddOrUpdateInventoryAsync(
                    entry.MaterialId,
                    yeniLoc,
                    yeniPers,
                    entry.Quantity,
                    "Kayıt güncellemesi (yeni stoğa ekle)",
                    currentUserName, cancellationToken);
            }

            await UpdateAsync(entry, cancellationToken);

            string hareketTipi = (miktarDegisti && !malzemeDegisti && !lokasyonDegisti && !birimDegisti && !personelDegisti) ? "Düzeltme" : "Düzenleme";
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
            if (silinecekEntry == null) return false;

            int? loc = silinecekEntry.LocationId != 0 ? silinecekEntry.LocationId : null;
            int? pers = (silinecekEntry.PersonelId.HasValue && silinecekEntry.PersonelId.Value != 0) ? silinecekEntry.PersonelId : null;

            // 1. Stoktan çıkar
            await _materialInventoryService.AddOrUpdateInventoryAsync(
                silinecekEntry.MaterialId,
                loc,
                pers,
                -silinecekEntry.Quantity,
                "MaterialEntry silindi, stoktan çıkarıldı",
                currentUserName,
                cancellationToken
            );

            // 2. Stok 0 veya altı ise sil (Temizlik)
            var stokKaydi = await _materialInventoryService.GetByMaterialLocationAsync(
                silinecekEntry.MaterialId,
                loc,
                pers,
                cancellationToken
            );
            if (stokKaydi != null && stokKaydi.Quantity <= 0)
            {
                await _materialInventoryService.DeleteAsync(stokKaydi, cancellationToken);
            }

            // 3. Kaydı sil
            await DeleteAsync(silinecekEntry, cancellationToken);

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