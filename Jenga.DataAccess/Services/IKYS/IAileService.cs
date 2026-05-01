using Jenga.Models.IKYS;
using Jenga.Utility.Results;

namespace Jenga.DataAccess.Services.IKYS;

public interface IAileService
{
    Task<Result<List<Aile>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<List<Aile>>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default);
    Task<Result<Aile>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result> AddAsync(Aile aile, string? modifiedBy = null, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(Aile aile, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Aile aile, CancellationToken cancellationToken = default);
}
