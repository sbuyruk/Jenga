using Jenga.Models.TBYS;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Jenga.DataAccess.Repositories.IRepository.TBYS
{
    public interface IBagisRepository : IRepository<Bagis>
    {
        Task<Bagis?> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default);
        Task<List<Bagis>> GetByBagisciIdAsync(int bagisciId, CancellationToken cancellationToken = default);
        Task<List<Bagis>> GetByTasinmazIdAsync(int tasinmazId, CancellationToken cancellationToken = default);
    }
}