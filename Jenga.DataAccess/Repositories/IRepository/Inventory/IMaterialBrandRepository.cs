using Jenga.Models.Inventory;

namespace Jenga.DataAccess.Repositories.IRepository.Inventory
{
    public interface IMaterialBrandRepository : IRepository<MaterialBrand>
    {
        Task<MaterialBrand?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    }
}