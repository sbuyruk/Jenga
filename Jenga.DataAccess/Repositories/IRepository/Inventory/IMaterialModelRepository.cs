using Jenga.Models.Inventory;

namespace Jenga.DataAccess.Repositories.IRepository.Inventory
{
    public interface IMaterialModelRepository : IRepository<MaterialModel>
    {
        Task<List<MaterialModel>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<MaterialModel?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task AddAsync(MaterialModel model, CancellationToken cancellationToken = default);
    }
}