using Jenga.Models.Inventory;
using Jenga.Utility.Results;

namespace Jenga.DataAccess.Services.Inventory
{
    public interface IMaterialModelService
    {
        Task<Result<List<MaterialModel>>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Result<MaterialModel>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result> AddAsync(MaterialModel model, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(MaterialModel model, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(MaterialModel model, CancellationToken cancellationToken = default);
    }
}