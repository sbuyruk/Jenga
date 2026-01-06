using Jenga.Models.Inventory;
using System.Threading;
using System.Threading.Tasks;

namespace Jenga.DataAccess.Repositories.IRepository.Inventory
{
    public interface IMaterialInventoryRepository : IRepository<MaterialInventory>
    {
        /// <summary>
        /// Finds an inventory row by material + optional location + optional person + optional brand + optional model.
        /// If a nullable parameter is null, matches rows with that column IS NULL.
        /// </summary>
        Task<MaterialInventory?> GetByMaterialLocationAsync(
            int materialId,
            int? locationId,
            int? personelId,
            int? brandId = null,
            int? modelId = null,
            CancellationToken cancellationToken = default);
    }
}