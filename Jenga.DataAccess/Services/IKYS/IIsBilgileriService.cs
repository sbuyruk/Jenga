using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Services.IKYS;

public interface IIsBilgileriService
{
    Task<List<IsBilgileri>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IsBilgileri?> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default);
    Task<IsBilgileri?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> AddAsync(IsBilgileri entity, string? modifiedBy = null, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(IsBilgileri entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(IsBilgileri entity, CancellationToken cancellationToken = default);
}
