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
        /// Malzeme, lokasyon ve birim kombinasyonu ile envanter kaydı getirir.
        /// </summary>
        Task<MaterialInventory?> GetByMaterialLocationAsync(int materialId, int locationId, int materialUnitId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Envanteri ekler veya günceller.
        /// </summary>
        Task AddOrUpdateInventoryAsync(
            int materialId,
            int locationId,
            int materialUnitId,
            int quantity,
            string aciklama,
            string degistirenKullanici,
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