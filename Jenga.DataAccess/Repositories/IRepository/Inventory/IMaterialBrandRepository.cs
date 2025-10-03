using Jenga.Models.Inventory;

namespace Jenga.DataAccess.Repositories.IRepository.Inventory
{
    public interface IMaterialBrandRepository : IRepository<MaterialBrand>
    {
        Task<List<MaterialBrand>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<MaterialBrand?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task AddAsync(MaterialBrand brand, CancellationToken cancellationToken = default);
    }
}