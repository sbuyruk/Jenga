using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jenga.Models.Inventory;

namespace Jenga.DataAccess.Repositories.IRepository.Inventory
{
    public interface IMaterialAssetRepository : IRepository<MaterialAsset>
    {
        Task<MaterialAsset?> GetBySerialNumberAsync(string serialNumber, CancellationToken cancellationToken = default);
        Task<MaterialAsset?> GetByBarcodeAsync(string barcode, CancellationToken cancellationToken = default);
        Task<List<MaterialAsset>> GetByMaterialIdAsync(int materialId, CancellationToken cancellationToken = default);
        Task<List<MaterialAsset>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default);
    }
}
