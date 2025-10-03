using Jenga.Models.Inventory;

namespace Jenga.DataAccess.Repositories.IRepository.Inventory
{
    public interface IMaterialCategoryRepository : IRepository<MaterialCategory>
    {
        // MaterialCategory’a özel metotlar ekleyebilirsin.
        Task<List<MaterialCategory>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<MaterialCategory?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task AddAsync(MaterialCategory category, CancellationToken cancellationToken = default);
    }
}