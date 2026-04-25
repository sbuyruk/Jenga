using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Services.IKYS;

public interface IIletisimBilgileriService
{
    Task<List<IletisimBilgileri>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IletisimBilgileri?> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default);
    Task<IletisimBilgileri?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> AddAsync(IletisimBilgileri entity, string? modifiedBy = null, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(IletisimBilgileri entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(IletisimBilgileri entity, CancellationToken cancellationToken = default);
}
