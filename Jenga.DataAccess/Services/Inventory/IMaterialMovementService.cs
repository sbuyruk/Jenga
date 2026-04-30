using Jenga.Models.Inventory;

namespace Jenga.DataAccess.Services.Inventory
{
    public interface IMaterialMovementService
    {
        Task AddAsync(MaterialMovement movement, CancellationToken cancellationToken = default);

        /// <summary>
        /// Belirli bir malzemenin hareket geçmişini getirir.
        /// </summary>
        Task<List<MaterialMovement>> GetMovementsByMaterialIdAsync(int materialId, CancellationToken cancellationToken = default);
    }
}