using Jenga.Models.TBYS;
using Jenga.Utility.Results;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.TBYS
{
    public interface ITasinmazBagisciService
    {
        Task<Result<List<TasinmazBagisci>>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Result<TasinmazBagisci>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<TasinmazBagisci>> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default);
        Task<Result> AddAsync(TasinmazBagisci bagisci, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(TasinmazBagisci bagisci, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(int bagisciId, CancellationToken cancellationToken = default);
        Task<Result<bool>> AnyAsync(Expression<Func<TasinmazBagisci, bool>> predicate, CancellationToken cancellationToken = default);
        Task<Result<(bool CanDelete, string? Reason)>> CanDeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<bool>> ExistsByTCKimlikAsync(long? tckimlik, int? excludeId = null, CancellationToken cancellationToken = default);
        Task<Result<int>> CountByIlIdsAsync(IEnumerable<int> ilIds, CancellationToken cancellationToken = default);
    }
}