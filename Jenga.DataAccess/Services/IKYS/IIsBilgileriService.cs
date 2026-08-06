using Jenga.Models.IKYS;
using Jenga.Utility.Results;
using System.Collections.Generic;

namespace Jenga.DataAccess.Services.IKYS;

public interface IIsBilgileriService
{
    Task<Result<List<IsBilgileri>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<IsBilgileri>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default);
    Task<Result<IsBilgileri>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<List<PersonelBolgeDashboardItem>>> GetPersonelForBolgeDashboardAsync(int bolgeId, CancellationToken cancellationToken = default);
    Task<Result> AddAsync(IsBilgileri entity, string? modifiedBy = null, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(IsBilgileri entity, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(IsBilgileri entity, CancellationToken cancellationToken = default);
}
