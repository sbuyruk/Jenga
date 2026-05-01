using Jenga.Models.FTK;
using Jenga.Utility.Results;

namespace Jenga.DataAccess.Services.FTK
{
    public interface IFtkKisiService
    {
        Task<Result<List<FtkKisi>>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Result<FtkKisi>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result> AddAsync(FtkKisi model, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(FtkKisi model, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(FtkKisi model, CancellationToken cancellationToken = default);
    }
}
