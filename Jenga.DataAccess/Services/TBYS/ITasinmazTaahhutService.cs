using Jenga.Models.TBYS;

namespace Jenga.DataAccess.Services.TBYS
{
    public interface ITasinmazTaahhutService
    {
        Task<List<TasinmazTaahhut>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<TasinmazTaahhut?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<List<TasinmazTaahhut>> GetByTasinmazIdAsync(int tasinmazId, CancellationToken cancellationToken = default);
        Task<List<TasinmazTaahhut>> GetByBagisciIdAsync(int bagisciId, CancellationToken cancellationToken = default);
        Task<bool> AddAsync(TasinmazTaahhut entity, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(TasinmazTaahhut entity, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
