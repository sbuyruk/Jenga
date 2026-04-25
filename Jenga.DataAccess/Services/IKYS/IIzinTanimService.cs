using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Services.IKYS;

public interface IIzinTanimService
{
    Task<List<IzinTanim>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IzinTanim?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> AddAsync(IzinTanim entity, string? modifiedBy = null, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(IzinTanim entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(IzinTanim entity, CancellationToken cancellationToken = default);
}
