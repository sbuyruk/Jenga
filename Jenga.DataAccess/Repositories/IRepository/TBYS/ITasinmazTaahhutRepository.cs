using Jenga.Models.TBYS;

namespace Jenga.DataAccess.Repositories.IRepository.TBYS
{
    public interface ITasinmazTaahhutRepository
    {
        Task<TasinmazTaahhut?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<List<TasinmazTaahhut>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<List<TasinmazTaahhut>> GetByTasinmazIdAsync(int tasinmazId, CancellationToken cancellationToken = default);
        Task<List<TasinmazTaahhut>> GetByBagisciIdAsync(int bagisciId, CancellationToken cancellationToken = default);
        Task AddAsync(TasinmazTaahhut entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(TasinmazTaahhut entity, CancellationToken cancellationToken = default);
        void Remove(TasinmazTaahhut entity);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
