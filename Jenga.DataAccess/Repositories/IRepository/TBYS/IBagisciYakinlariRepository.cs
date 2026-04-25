using Jenga.Models.TBYS;

namespace Jenga.DataAccess.Repositories.IRepository.TBYS
{
    public interface IBagisciYakinlariRepository : IRepository<BagisciYakinlari>
    {
        Task<List<BagisciYakinlari>> GetByBagisciIdAsync(long bagisciId, CancellationToken cancellationToken = default);
    }
}
