using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jenga.Models.NBYS;

namespace Jenga.DataAccess.Services.NBYS
{
    public interface IArmaganService
    {
        Task<List<Armagan>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Armagan?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> AddAsync(Armagan model, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(Armagan model, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(Armagan model, CancellationToken cancellationToken = default);
    }
}
