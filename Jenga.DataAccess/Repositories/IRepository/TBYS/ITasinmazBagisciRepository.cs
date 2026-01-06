using Jenga.Models.TBYS;
using System.Threading;
using System.Threading.Tasks;

namespace Jenga.DataAccess.Repositories.IRepository.TBYS
{
    public interface ITasinmazBagisciRepository : IRepository<TasinmazBagisci>
    {
        Task<TasinmazBagisci?> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default);
    }
}
