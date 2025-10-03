using Jenga.Models.Inventory;

namespace Jenga.DataAccess.Repositories.IRepository.Inventory
{
    public interface IMaterialMovementRepository : IRepository<MaterialMovement>
    {
        Task<List<MaterialMovement>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<MaterialMovement?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task AddAsync(MaterialMovement movement, CancellationToken cancellationToken = default);
    }
}