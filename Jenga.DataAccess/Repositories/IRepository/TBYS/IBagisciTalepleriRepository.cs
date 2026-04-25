using Jenga.Models.TBYS;

namespace Jenga.DataAccess.Repositories.IRepository.TBYS
{
    public interface IBagisciTalepleriRepository : IRepository<BagisciTalepleri>
    {
        Task<List<BagisciTalepleri>> GetByBagisciIdAsync(long bagisciId, CancellationToken cancellationToken = default);
    }
}
