using Jenga.Models.Inventory;

namespace Jenga.DataAccess.Services.Inventory
{
    public interface IMaterialModelService
    {
        Task<List<MaterialModel>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<MaterialModel?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> AddAsync(MaterialModel model, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(MaterialModel model, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(MaterialModel model, CancellationToken cancellationToken = default);
    }
}