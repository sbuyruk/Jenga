using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Services.IKYS;

public interface IGorevTanimService
{
    Task<List<GorevTanim>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<GorevTanim>> GetByBirimIdAsync(int birimId, CancellationToken cancellationToken = default);
    Task<GorevTanim?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> AddAsync(GorevTanim entity, string? modifiedBy = null, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(GorevTanim entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(GorevTanim entity, CancellationToken cancellationToken = default);
}
