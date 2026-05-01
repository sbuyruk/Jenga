using Jenga.Models.IKYS;
using Jenga.Utility.Results;

namespace Jenga.DataAccess.Services.IKYS;

public interface IYabanciDilService
{
    Task<Result<List<YabanciDil>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<List<YabanciDil>>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default);
    Task<Result<YabanciDil>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result> AddAsync(YabanciDil entity, string? modifiedBy = null, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(YabanciDil entity, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(YabanciDil entity, CancellationToken cancellationToken = default);
}
