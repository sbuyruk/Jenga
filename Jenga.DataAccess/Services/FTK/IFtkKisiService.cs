using Jenga.Models.FTK;

namespace Jenga.DataAccess.Services.FTK
{
    public interface IFtkKisiService
    {
        Task<List<FtkKisi>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<FtkKisi?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> AddAsync(FtkKisi model, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(FtkKisi model, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(FtkKisi model, CancellationToken cancellationToken = default);
    }
}
