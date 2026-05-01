using Jenga.Models.IKYS;
using Jenga.Utility.Results;

namespace Jenga.DataAccess.Services.IKYS;

public interface IBirimTanimService
{
    Task<Result<List<BirimTanim>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<BirimTanim>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result> AddAsync(BirimTanim entity, string? modifiedBy = null, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(BirimTanim entity, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(BirimTanim entity, CancellationToken cancellationToken = default);
}
