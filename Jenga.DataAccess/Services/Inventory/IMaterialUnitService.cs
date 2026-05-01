using Jenga.Models.Inventory;
using Jenga.Utility.Results;

namespace Jenga.DataAccess.Services.Inventory
{
    public interface IMaterialUnitService
    {
        Task<Result<List<MaterialUnit>>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Result<MaterialUnit>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result> AddAsync(MaterialUnit unit, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(MaterialUnit unit, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(MaterialUnit unit, CancellationToken cancellationToken = default);
    }
}