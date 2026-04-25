using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Services.IKYS;

public interface IGorevOnayService
{
    Task<List<GorevOnay>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<GorevOnay>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default);
    Task<GorevOnay?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> AddAsync(GorevOnay entity, string? modifiedBy = null, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(GorevOnay entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(GorevOnay entity, CancellationToken cancellationToken = default);
}
