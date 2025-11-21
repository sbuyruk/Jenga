using Jenga.Models.Inventory;
using System.Threading;
using System.Threading.Tasks;

namespace Jenga.DataAccess.Repositories.IRepository.Inventory
{
    public interface IMaterialInventoryRepository : IRepository<MaterialInventory>
    {
        /// <summary>
        /// Finds an inventory row by material + optional location + optional person.
        /// If locationId is null, matches rows with LocationId IS NULL.
        /// If personelId is null, matches rows with PersonelId IS NULL.
        /// </summary>
        Task<MaterialInventory?> GetByMaterialLocationAsync(int materialId, int? locationId, int? personelId, CancellationToken cancellationToken = default);
    }
}