using Jenga.Models.Inventory;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.Inventory
{
    public interface IMaterialInventoryService
    {
        /// <summary>
        /// Tüm envanter kayıtlarını getirir.
        /// </summary>
        Task<List<MaterialInventory>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Predicate ile herhangi bir envanter kaydı olup olmadığını kontrol eder (örn. silme guard'ları için).
        /// </summary>
        Task<bool> AnyAsync(Expression<Func<MaterialInventory, bool>> predicate);
    }
}