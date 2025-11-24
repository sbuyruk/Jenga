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

            // 1. Transfer kaydını ekle
            await _unitOfWork.MaterialTransfer.AddAsync(transfer, cancellationToken);
            await _unitOfWork.MaterialTransfer.SaveChangesAsync(cancellationToken);

            // 2. ID'leri temizle (0 -> null)
            // UI'dan gelen veriye güveniyoruz ve 0 olan ID'leri null yapıyoruz.
            int? actualFromLocation = transfer.FromLocationId != 0 ? transfer.FromLocationId : null;
            int? actualToLocation = transfer.ToLocationId != 0 ? transfer.ToLocationId : null;

            // PersonelId'ler için de 0 kontrolü
            int? actualFromPerson = (transfer.FromPersonId.HasValue && transfer.FromPersonId != 0) ? transfer.FromPersonId : null;
            int? actualToPerson = (transfer.ToPersonId.HasValue && transfer.ToPersonId != 0) ? transfer.ToPersonId : null;

            // 3. KAYNAK stoktan düş
            await _materialInventoryService.AddOrUpdateInventoryAsync(
                transfer.MaterialId,
                actualFromLocation,
                actualFromPerson,
                -transfer.Quantity,
                "MaterialTransfer: Kaynak stoktan düşüldü.",
                modifiedBy,
                cancellationToken);

            // 4. HEDEF stoğa ekle
            await _materialInventoryService.AddOrUpdateInventoryAsync(
                transfer.MaterialId,
                actualToLocation,
                actualToPerson,
                transfer.Quantity,
                "MaterialTransfer: Hedef stoğa eklendi.",
                modifiedBy,
                cancellationToken);

            // 5. Hareket Logu
            await _materialMovementService.AddAsync(new MaterialMovement
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
                OlusturmaTarihi = DateTime.Now
            }, cancellationToken);

            return true;
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