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
        /// Yeni: Eğer MaterialEntry.PersonelId varsa ToPersonId'yi doldurur.
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
                MovementDate = DateTime.Now,
                Aciklama = aciklama,
                Olusturan = userName,
                OlusturmaTarihi = DateTime.Now
            };

            // Eğer entry üzerinde PersonelId property varsa (yeni field), ToPersonId olarak set et
            var prop = entry.GetType().GetProperty("PersonelId");
            if (prop != null)
            {
                var val = prop.GetValue(entry);
                if (val != null)
                {
                    if (val is int ival) movement.ToPersonId = ival;
                    else if (val is int?) movement.ToPersonId = (int?)val;
                }
            }

            await AddAsync(movement, cancellationToken);
        }
    }
}