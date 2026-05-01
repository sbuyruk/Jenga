using Jenga.Models.IKYS;
using Jenga.Utility.Results;

namespace Jenga.DataAccess.Services.IKYS;

public interface IGorevTanimService
{
    Task<Result<List<GorevTanim>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<List<GorevTanim>>> GetByBirimIdAsync(int birimId, CancellationToken cancellationToken = default);
    Task<Result<GorevTanim>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result> AddAsync(GorevTanim entity, string? modifiedBy = null, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(GorevTanim entity, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(GorevTanim entity, CancellationToken cancellationToken = default);
}
