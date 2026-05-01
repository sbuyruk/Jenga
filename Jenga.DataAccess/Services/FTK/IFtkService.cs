using Jenga.Models.FTK;
using Jenga.Utility.Results;

namespace Jenga.DataAccess.Services.FTK
{
    public interface IFtkService
    {
        Task<Result<List<Ftk>>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Result<List<Ftk>>> GetLatestPerIslemAsync(CancellationToken cancellationToken = default);
        Task<Result<Ftk>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result> AddAsync(Ftk model, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(Ftk model, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(Ftk model, CancellationToken cancellationToken = default);
    }
}
