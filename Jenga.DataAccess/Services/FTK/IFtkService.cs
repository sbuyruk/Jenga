using Jenga.Models.FTK;

namespace Jenga.DataAccess.Services.FTK
{
    public interface IFtkService
    {
        Task<List<Ftk>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<List<Ftk>> GetLatestPerIslemAsync(CancellationToken cancellationToken = default);
        Task<Ftk?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> AddAsync(Ftk model, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(Ftk model, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(Ftk model, CancellationToken cancellationToken = default);
    }
}
