using Jenga.Models.Common;

namespace Jenga.DataAccess.Services.Common
{
    public interface IIlceService
    {
        Task<List<Ilce>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Ilce?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<List<Ilce>> GetByIlIdAsync(int ilId, CancellationToken cancellationToken = default);
        Task<List<Ilce>> GetAktifIlcelerAsync(CancellationToken cancellationToken = default);    
    }
}
