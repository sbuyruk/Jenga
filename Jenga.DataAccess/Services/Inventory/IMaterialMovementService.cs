using Jenga.Models.Inventory;
using Jenga.Utility.Results;

namespace Jenga.DataAccess.Services.Inventory
{
    public interface IMaterialMovementService
    {
        Task<Result> AddAsync(MaterialMovement movement, CancellationToken cancellationToken = default);

        /// <summary>
        /// Belirli bir malzemenin hareket geçmişini getirir.
        /// </summary>
        Task<Result<List<MaterialMovement>>> GetMovementsByMaterialIdAsync(int materialId, CancellationToken cancellationToken = default);
    }
}