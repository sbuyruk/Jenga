using Jenga.Models.IKYS;
using Jenga.Utility.Results;

namespace Jenga.DataAccess.Services.IKYS;

public interface IPersonelSummaryService
{
    Task<Result<PersonelSummary>> GetAsync(int personelId, CancellationToken cancellationToken = default);
}
