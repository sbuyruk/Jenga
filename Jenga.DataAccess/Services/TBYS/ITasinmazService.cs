using Jenga.Models.TBYS;
using Jenga.Utility.Results;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.TBYS
{
    public interface ITasinmazService
    {
        Task<Result<List<Tasinmaz>>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Result<List<Tasinmaz>>> GetByEnvanterDurumuAsync(int envanterdeMi, CancellationToken cancellationToken = default);
        Task<Result<List<Tasinmaz>>> GetEnvanterdekilerAsync(CancellationToken cancellationToken = default);

        Task<Result<Tasinmaz>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result> AddAsync(Tasinmaz tasinmaz, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(Tasinmaz tasinmaz, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(int tasinmazId, CancellationToken cancellationToken = default);
        Task<Result<bool>> AnyAsync(Expression<Func<Tasinmaz, bool>> predicate, CancellationToken cancellationToken = default);
        Task<Result<(bool CanDelete, string? Reason)>> CanDeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<bool>> ExistsByEmlakSicilNoAsync(string emlakSicilNo, int? excludeId = null, CancellationToken cancellationToken = default);
        Task<Result<string>> GetEmlakSicilNoAsync(int id, CancellationToken cancellationToken = default);
    }
}
