using Jenga.Models.Inventory;

namespace Jenga.DataAccess.Services.Inventory
{
    public interface IMaterialCategoryService
    {
        Task<List<MaterialCategory>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<MaterialCategory?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> AddAsync(MaterialCategory category, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(MaterialCategory category, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(MaterialCategory category, CancellationToken cancellationToken = default);
    }
}