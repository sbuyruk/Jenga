using Jenga.Models.NBYS;
using Jenga.Utility.Results;
using System.Collections.Generic;

namespace Jenga.DataAccess.Services.NBYS
{
    public interface INakitBagisciService
    {
        Task<Result<List<NakitBagisci>>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Result<List<NakitBagisciDashboardItem>>> GetAllForDashboardAsync(CancellationToken cancellationToken = default);
        Task<Result<List<NakitBagisciDashboardItem>>> GetAllForBolgeDashboardAsync(IEnumerable<int> ilIds, CancellationToken cancellationToken = default);
        Task<Result<List<NakitBagisciArmaganItem>>> GetAllForArmaganDashboardAsync(CancellationToken cancellationToken = default);
        Task<Result<NakitBagisci>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result> AddAsync(NakitBagisci model, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(NakitBagisci model, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(NakitBagisci model, CancellationToken cancellationToken = default);
    }
}
