using Jenga.Models.NBYS;

namespace Jenga.DataAccess.Services.NBYS
{
    public interface IDuzenliNakitBagisciService
    {
        Task<List<DuzenliNakitBagisci>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<DuzenliNakitBagisci?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> AddAsync(DuzenliNakitBagisci model, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(DuzenliNakitBagisci model, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(DuzenliNakitBagisci model, CancellationToken cancellationToken = default);
    }
}
