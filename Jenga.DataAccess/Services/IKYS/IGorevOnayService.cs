using Jenga.Models.IKYS;
using Jenga.Utility.Results;

namespace Jenga.DataAccess.Services.IKYS;

public interface IGorevOnayService
{
    Task<Result<List<GorevOnay>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<List<GorevOnay>>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default);
    Task<Result<GorevOnay>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result> AddAsync(GorevOnay entity, string? modifiedBy = null, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(GorevOnay entity, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(GorevOnay entity, CancellationToken cancellationToken = default);
}
