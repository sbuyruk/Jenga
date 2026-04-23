using Jenga.Models.FTK;

namespace Jenga.DataAccess.Services.FTK
{
    public interface IFtkIslemService
    {
        Task<List<FtkIslem>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<FtkIslem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> AddAsync(FtkIslem model, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(FtkIslem model, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(FtkIslem model, CancellationToken cancellationToken = default);
    }
}
