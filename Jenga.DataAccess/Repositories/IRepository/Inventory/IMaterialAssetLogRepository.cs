using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jenga.Models.Inventory;

namespace Jenga.DataAccess.Repositories.IRepository.Inventory
{
    public interface IMaterialAssetLogRepository : IRepository<MaterialAssetLog>
    {
        Task<List<MaterialAssetLog>> GetByAssetIdAsync(int materialAssetId, CancellationToken cancellationToken = default);
    }
}
