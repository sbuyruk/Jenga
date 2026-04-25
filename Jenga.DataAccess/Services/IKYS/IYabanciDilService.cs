using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Services.IKYS;

public interface IYabanciDilService
{
    Task<List<YabanciDil>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<YabanciDil>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default);
    Task<YabanciDil?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> AddAsync(YabanciDil entity, string? modifiedBy = null, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(YabanciDil entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(YabanciDil entity, CancellationToken cancellationToken = default);
}
