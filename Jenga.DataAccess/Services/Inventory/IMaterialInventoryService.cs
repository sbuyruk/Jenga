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
        /// Malzeme + (opsiyonel) lokasyon + (opsiyonel) personel + (opsiyonel) brand + (opsiyonel) model kombinasyonu ile envanter kaydı getirir.
        /// locationId, personelId, brandId veya modelId null olabilir.
        /// </summary>
        Task<MaterialInventory?> GetByMaterialLocationAsync(int materialId, int? locationId, int? personelId, int? brandId = null, int? modelId = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Envanteri ekler veya günceller.
        /// locationId, personelId, brandId ve modelId nullable olabilir.
        /// </summary>
        Task AddOrUpdateInventoryAsync(
            int materialId,
            int? locationId,
            int? personelId,
            int quantity,
            string aciklama,
            string? degistirenKullanici,
            int? brandId = null,
            int? modelId = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Belirli Id ile envanter kaydını getirir.
        /// </summary>
        Task<MaterialInventory?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Envanter kaydını günceller.
        /// </summary>
        Task UpdateInventoryAsync(MaterialInventory inventory, string degistirenKullanici, CancellationToken cancellationToken = default);

        /// <summary>
        /// Yeni envanter kaydı ekler.
        /// </summary>
        Task AddAsync(MaterialInventory inventory, CancellationToken cancellationToken = default);

        Task DeleteAsync(MaterialInventory inventory, CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(Expression<Func<MaterialInventory, bool>> predicate);
    }
}