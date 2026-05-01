using Jenga.Models.Common;
using Jenga.Utility.Results;

namespace Jenga.DataAccess.Services.Common
{
    public interface IIlceService
    {
        Task<Result<List<Ilce>>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Result<Ilce>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<List<Ilce>>> GetByIlIdAsync(int ilId, CancellationToken cancellationToken = default);
        Task<Result<List<Ilce>>> GetAktifIlcelerAsync(CancellationToken cancellationToken = default);
    }
}
