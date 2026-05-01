using Jenga.Models.NBYS;
using Jenga.Utility.Results;

namespace Jenga.DataAccess.Services.NBYS
{
    public interface IArmaganService
    {
        Task<Result<List<Armagan>>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Result<Armagan>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result> AddAsync(Armagan model, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(Armagan model, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(Armagan model, CancellationToken cancellationToken = default);
    }
}
