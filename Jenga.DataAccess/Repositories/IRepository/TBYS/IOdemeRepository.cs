using Jenga.Models.TBYS;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Jenga.DataAccess.Repositories.IRepository.TBYS
{
    public interface IOdemeRepository : IRepository<Odeme>
    {
        Task<Odeme?> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default);
        Task<List<Odeme>> GetBySozlesmeIdAsync(int sozlesmeId, CancellationToken cancellationToken = default);
        Task<List<Odeme>> GetByKiraciIdAsync(int kiraciId, CancellationToken cancellationToken = default);
        Task<List<Odeme>> GetByOdemePlaniIdAsync(int odemePlaniId, CancellationToken cancellationToken = default);
        Task<List<Odeme>> GetAllWithOdemePlaniAsync(CancellationToken cancellationToken = default);
    }
}
