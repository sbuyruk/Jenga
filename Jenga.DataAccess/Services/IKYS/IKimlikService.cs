using Jenga.Models.IKYS;
using Jenga.Utility.Results;

namespace Jenga.DataAccess.Services.IKYS;

public interface IKimlikService
{
    Task<Result<List<Kimlik>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<Kimlik>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default);
    Task<Result<Kimlik>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result> AddAsync(Kimlik entity, string? modifiedBy = null, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(Kimlik entity, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Kimlik entity, CancellationToken cancellationToken = default);
}
