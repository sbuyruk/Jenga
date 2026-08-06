using Jenga.Models.NBYS;
using Jenga.Utility.Results;
using System.Collections.Generic;

namespace Jenga.DataAccess.Services.NBYS
{
    public interface IArmaganService
    {
        Task<Result<List<Armagan>>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Result<List<ArmaganDashboardItem>>> GetAllForDashboardAsync(CancellationToken cancellationToken = default);
        Task<Result<List<ArmaganDashboardItem>>> GetAllForBolgeDashboardAsync(IEnumerable<int> bagisciIds, CancellationToken cancellationToken = default);
        Task<Result<Armagan>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result> AddAsync(Armagan model, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(Armagan model, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(Armagan model, CancellationToken cancellationToken = default);
    }
}
