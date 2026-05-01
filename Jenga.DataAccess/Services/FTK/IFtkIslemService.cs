using Jenga.Models.FTK;
using Jenga.Utility.Results;

namespace Jenga.DataAccess.Services.FTK
{
    public interface IFtkIslemService
    {
        Task<Result<List<FtkIslem>>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Result<FtkIslem>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result> AddAsync(FtkIslem model, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(FtkIslem model, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(FtkIslem model, CancellationToken cancellationToken = default);
    }
}
