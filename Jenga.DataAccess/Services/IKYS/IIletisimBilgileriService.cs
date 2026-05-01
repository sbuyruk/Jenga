using Jenga.Models.IKYS;
using Jenga.Utility.Results;

namespace Jenga.DataAccess.Services.IKYS;

public interface IIletisimBilgileriService
{
    Task<Result<List<IletisimBilgileri>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<IletisimBilgileri>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default);
    Task<Result<IletisimBilgileri>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result> AddAsync(IletisimBilgileri entity, string? modifiedBy = null, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(IletisimBilgileri entity, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(IletisimBilgileri entity, CancellationToken cancellationToken = default);
}
