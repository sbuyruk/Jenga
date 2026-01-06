using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.Inventory;

namespace Jenga.DataAccess.Services.Inventory
{
    public class MaterialMovementService : IMaterialMovementService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MaterialMovementService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<MaterialMovement>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _unitOfWork.MaterialMovement.GetAllAsync(cancellationToken);

        public async Task<MaterialMovement?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => await _unitOfWork.MaterialMovement.GetByIdAsync(id, cancellationToken);

        public async Task AddAsync(MaterialMovement movement, CancellationToken cancellationToken = default)
        {
            // Ensure MovementDate and a sensible Operation value if caller didn't set them.
            if (movement.MovementDate == default) movement.MovementDate = DateTime.Now;

            movement.Operation ??= !string.IsNullOrWhiteSpace(movement.MovementType)
                ? movement.MovementType
                : (movement.FromLocationId.HasValue && movement.ToLocationId.HasValue ? "Transfer"
                    : (!movement.FromLocationId.HasValue && movement.ToLocationId.HasValue ? "Giriş"
                        : (movement.FromLocationId.HasValue && !movement.ToLocationId.HasValue ? "Çıkış" : "Diğer")));

            await _unitOfWork.MaterialMovement.AddAsync(movement, cancellationToken);
            await _unitOfWork.MaterialMovement.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(MaterialMovement movement, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.MaterialMovement.UpdateAsync(movement);
            await _unitOfWork.MaterialMovement.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(MaterialMovement movement, CancellationToken cancellationToken = default)
        {
            _unitOfWork.MaterialMovement.Remove(movement);
            await _unitOfWork.MaterialMovement.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<MaterialMovement>> GetMovementsByMaterialIdAsync(int materialId, CancellationToken cancellationToken = default)
        {
            var list = await _unitOfWork.MaterialMovement.GetAllAsync(cancellationToken);
            return list.Where(x => x.MaterialId == materialId).ToList();
        }

        public async Task<List<MaterialMovement>> GetMovementsByDateRangeAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default)
        {
            var list = await _unitOfWork.MaterialMovement.GetAllAsync(cancellationToken);
            return list.Where(x => x.MovementDate >= start && x.MovementDate <= end).ToList();
        }

        /// <summary>
        /// MaterialEntry CRUD işlemlerinde otomatik hareket logu ekler.
        /// Yeni: Eğer MaterialEntry.PersonelId/BrandId/ModelId varsa ToPersonId/BrandId/ModelId olarak set eder.
        /// </summary>
        public async Task AddMovementForEntryAsync(MaterialEntry entry, string movementType, string? aciklama, string? userName, CancellationToken cancellationToken = default)
        {
            var movement = new MaterialMovement
            {
                MaterialId = entry.MaterialId,
                Quantity = entry.Quantity,
                MaterialUnitId = entry.MaterialUnitId,
                FromLocationId = null, // Girişte null, çıkışta dolu olabilir
                ToLocationId = entry.LocationId,
                MovementType = movementType,
                Operation = movementType, // set Operation from provided movementType
                MovementDate = entry.EntryDate,
                Aciklama = aciklama,
                Olusturan = userName,
                OlusturmaTarihi = DateTime.Now
            };

            // If entry has PersonelId property, set ToPersonId
            var personProp = entry.GetType().GetProperty("PersonelId");
            if (personProp != null)
            {
                var val = personProp.GetValue(entry);
                if (val != null)
                {
                    if (val is int ival) movement.ToPersonId = ival;
                    else if (val is int?) movement.ToPersonId = (int?)val;
                }
            }

            // If entry has BrandId/ModelId properties, set them on movement
            var brandProp = entry.GetType().GetProperty("BrandId");
            if (brandProp != null)
            {
                var val = brandProp.GetValue(entry);
                if (val != null)
                {
                    if (val is int bval) movement.BrandId = bval;
                    else if (val is int?) movement.BrandId = (int?)val;
                }
            }

            var modelProp = entry.GetType().GetProperty("ModelId");
            if (modelProp != null)
            {
                var val = modelProp.GetValue(entry);
                if (val != null)
                {
                    if (val is int mval) movement.ModelId = mval;
                    else if (val is int?) movement.ModelId = (int?)val;
                }
            }

            await AddAsync(movement, cancellationToken);
        }
    }
}