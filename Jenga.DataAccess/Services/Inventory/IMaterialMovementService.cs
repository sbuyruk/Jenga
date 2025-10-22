using Jenga.Models.Inventory;

namespace Jenga.DataAccess.Services.Inventory
{
    public interface IMaterialMovementService
    {
        Task<List<MaterialMovement>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<MaterialMovement?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task AddAsync(MaterialMovement movement, CancellationToken cancellationToken = default);
        Task UpdateAsync(MaterialMovement movement, CancellationToken cancellationToken = default);
        Task DeleteAsync(MaterialMovement movement, CancellationToken cancellationToken = default);

        /// <summary>
        /// Belirli bir malzemenin hareket geçmişini getirir.
        /// </summary>
        Task<List<MaterialMovement>> GetMovementsByMaterialIdAsync(int materialId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Tarih aralığına göre hareketleri getirir.
        /// </summary>
        Task<List<MaterialMovement>> GetMovementsByDateRangeAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default);

        /// <summary>
        /// MaterialEntry CRUD işlemlerinde otomatik hareket logu ekler (isteğe bağlı).
        /// </summary>
        Task AddMovementForEntryAsync(MaterialEntry entry, string movementType, string? aciklama, string? userName, CancellationToken cancellationToken = default);
    }
}