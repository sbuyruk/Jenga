using Jenga.Models.FTK;
using Jenga.Utility.Results;
using System.Collections.Generic;

namespace Jenga.DataAccess.Services.FTK
{
    public interface IFtkIslemService
    {
        Task<Result<List<FtkIslem>>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Result<List<FtkIslemDashboardItem>>> GetForBolgeDashboardAsync(IEnumerable<int> ftkIslemIds, CancellationToken cancellationToken = default);
        Task<Result<FtkIslem>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result> AddAsync(FtkIslem model, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(FtkIslem model, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(FtkIslem model, CancellationToken cancellationToken = default);
    }
}
