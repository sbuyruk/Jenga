using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jenga.Models.NBYS;

namespace Jenga.DataAccess.Services.NBYS
{
    public interface INakitBagisciService
    {
        Task<List<NakitBagisci>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<NakitBagisci?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> AddAsync(NakitBagisci model, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(NakitBagisci model, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(NakitBagisci model, CancellationToken cancellationToken = default);
    }
}
