using Jenga.Models.TBYS;
using Jenga.Utility.Results;

namespace Jenga.DataAccess.Services.TBYS
{
    public interface ITasinmazTaahhutService
    {
        Task<Result<List<TasinmazTaahhut>>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Result<TasinmazTaahhut>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<List<TasinmazTaahhut>>> GetByTasinmazIdAsync(int tasinmazId, CancellationToken cancellationToken = default);
        Task<Result<List<TasinmazTaahhut>>> GetByBagisciIdAsync(int bagisciId, CancellationToken cancellationToken = default);
        Task<Result> AddAsync(TasinmazTaahhut entity, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(TasinmazTaahhut entity, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
