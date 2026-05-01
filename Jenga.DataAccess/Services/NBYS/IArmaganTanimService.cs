using Jenga.Models.NBYS;
using Jenga.Utility.Results;

namespace Jenga.DataAccess.Services.NBYS
{
    public interface IArmaganTanimService
    {
        Task<Result<List<ArmaganTanim>>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Result<ArmaganTanim>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result> AddAsync(ArmaganTanim model, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(ArmaganTanim model, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(ArmaganTanim model, CancellationToken cancellationToken = default);
    }
}
