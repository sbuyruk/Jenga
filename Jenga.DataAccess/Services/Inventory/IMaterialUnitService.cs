using Jenga.Models.Inventory;

namespace Jenga.DataAccess.Services.Inventory
{
    public interface IMaterialUnitService
    {
        Task<List<MaterialUnit>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<MaterialUnit?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> AddAsync(MaterialUnit unit, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(MaterialUnit unit, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(MaterialUnit unit, CancellationToken cancellationToken = default);
    }
}