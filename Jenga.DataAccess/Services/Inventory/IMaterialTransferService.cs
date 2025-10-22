using Jenga.Models.Inventory;

namespace Jenga.DataAccess.Services.Inventory
{
    public interface IMaterialTransferService
    {
        Task<List<MaterialTransfer>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<MaterialTransfer?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task AddAsync(MaterialTransfer transfer, CancellationToken cancellationToken = default);
        Task UpdateAsync(MaterialTransfer transfer, CancellationToken cancellationToken = default);
        Task DeleteAsync(MaterialTransfer transfer, CancellationToken cancellationToken = default);
        // Ek filtre/sorgu fonksiyonları eklenebilir.
    }
}