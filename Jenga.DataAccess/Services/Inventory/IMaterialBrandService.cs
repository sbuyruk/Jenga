using Jenga.Models.Inventory;

namespace Jenga.DataAccess.Services.Inventory
{
    public interface IMaterialBrandService
    {
        Task<List<MaterialBrand>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<MaterialBrand?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> AddAsync(MaterialBrand brand, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(MaterialBrand brand, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(MaterialBrand brand, CancellationToken cancellationToken = default);
    }
}