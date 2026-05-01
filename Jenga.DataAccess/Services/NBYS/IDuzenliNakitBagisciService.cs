using Jenga.Models.NBYS;
using Jenga.Utility.Results;

namespace Jenga.DataAccess.Services.NBYS
{
    public interface IDuzenliNakitBagisciService
    {
        Task<Result<List<DuzenliNakitBagisci>>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Result<DuzenliNakitBagisci>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result> AddAsync(DuzenliNakitBagisci model, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(DuzenliNakitBagisci model, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(DuzenliNakitBagisci model, CancellationToken cancellationToken = default);
    }
}
