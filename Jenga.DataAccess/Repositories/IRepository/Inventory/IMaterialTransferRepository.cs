using Jenga.Models.Inventory;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Repositories.IRepository.Inventory
{
    public interface IMaterialTransferRepository : IRepository<MaterialTransfer>
    {
        // Ekstra transfer sorguları eklenebilir.
        Task<bool> AnyAsync(Expression<Func<MaterialTransfer, bool>> predicate, CancellationToken cancellationToken = default);
    }
}