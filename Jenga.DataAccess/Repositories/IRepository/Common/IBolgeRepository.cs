using Jenga.Models.Common;

namespace Jenga.DataAccess.Repositories.IRepository.Common
{
    public interface IBolgeRepository : IRepository<Bolge>
    {
        Task<Bolge?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    }
}