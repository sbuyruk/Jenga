using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jenga.Models.NBYS;

namespace Jenga.DataAccess.Services.NBYS
{
    public interface INakitBagisHareketService
    {
        Task<List<NakitBagisHareket>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<List<NakitBagisHareket>> GetLastYearsAsync(int years, CancellationToken cancellationToken = default);
        Task<NakitBagisHareket?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> AddAsync(NakitBagisHareket model, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(NakitBagisHareket model, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(NakitBagisHareket model, CancellationToken cancellationToken = default);
    }
}
