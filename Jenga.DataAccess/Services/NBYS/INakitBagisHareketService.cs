using Jenga.Models.NBYS;
using Jenga.Utility.Results;

namespace Jenga.DataAccess.Services.NBYS
{
    public interface INakitBagisHareketService
    {
        Task<Result<List<NakitBagisHareket>>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Result<List<NakitBagisHareket>>> GetLastYearsAsync(int years, CancellationToken cancellationToken = default);
        Task<Result<List<NakitBagisDashboardItem>>> GetLastYearsForDashboardAsync(int years, CancellationToken cancellationToken = default);
        Task<Result<NakitBagisHareket>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result> AddAsync(NakitBagisHareket model, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(NakitBagisHareket model, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(NakitBagisHareket model, CancellationToken cancellationToken = default);
    }
}
