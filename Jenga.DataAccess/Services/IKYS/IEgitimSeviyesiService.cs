using Jenga.Models.IKYS;
using Jenga.Utility.Results;

namespace Jenga.DataAccess.Services.IKYS;

public interface IEgitimSeviyesiService
{
    Task<Result<List<EgitimSeviyesi>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<EgitimSeviyesi>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result> AddAsync(EgitimSeviyesi entity, string? modifiedBy = null, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(EgitimSeviyesi entity, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(EgitimSeviyesi entity, CancellationToken cancellationToken = default);
}
