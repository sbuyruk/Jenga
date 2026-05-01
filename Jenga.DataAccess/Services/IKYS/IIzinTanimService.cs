using Jenga.Models.IKYS;
using Jenga.Utility.Results;

namespace Jenga.DataAccess.Services.IKYS;

public interface IIzinTanimService
{
    Task<Result<List<IzinTanim>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<IzinTanim>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result> AddAsync(IzinTanim entity, string? modifiedBy = null, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(IzinTanim entity, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(IzinTanim entity, CancellationToken cancellationToken = default);
}
