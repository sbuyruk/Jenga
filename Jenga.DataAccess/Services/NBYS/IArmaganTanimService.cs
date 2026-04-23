using Jenga.Models.NBYS;

namespace Jenga.DataAccess.Services.NBYS
{
    public interface IArmaganTanimService
    {
        Task<List<ArmaganTanim>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<ArmaganTanim?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> AddAsync(ArmaganTanim model, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(ArmaganTanim model, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(ArmaganTanim model, CancellationToken cancellationToken = default);
    }
}
