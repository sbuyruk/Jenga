using Jenga.Models.Inventory;
using Jenga.Models.TBYS;

namespace Jenga.DataAccess.Repositories.IRepository.TBYS
{
    public interface ITasinmazRepository : IRepository<Tasinmaz>
    {
        Task<Tasinmaz?> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default);
    }
}