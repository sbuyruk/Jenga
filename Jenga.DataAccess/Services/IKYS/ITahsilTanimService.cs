using Jenga.Models.IKYS;
using Jenga.Utility.Results;

namespace Jenga.DataAccess.Services.IKYS;

public interface ITahsilTanimService
{
    Task<Result<List<TahsilTanim>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<TahsilTanim>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result> AddAsync(TahsilTanim entity, string? modifiedBy = null, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(TahsilTanim entity, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(TahsilTanim entity, CancellationToken cancellationToken = default);
}
