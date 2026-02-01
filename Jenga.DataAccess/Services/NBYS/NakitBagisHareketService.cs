using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.NBYS;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.NBYS
{
    public class NakitBagisHareketService : INakitBagisHareketService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public NakitBagisHareketService(IUnitOfWork unitOfWork, IDbContextFactory<ApplicationDbContext> dbFactory)
        {
            _unitOfWork = unitOfWork;
            _dbFactory = dbFactory;
        }

        public Task<List<NakitBagisHareket>> GetAllAsync(CancellationToken cancellationToken = default)
            => _unitOfWork.NakitBagisHareket.GetAllAsync(cancellationToken);

        public async Task<List<NakitBagisHareket>> GetLastYearsAsync(int years, CancellationToken cancellationToken = default)
        {
            var startDate = DateTime.Today.AddYears(-years);

            await using var db = _dbFactory.CreateDbContext();
            return await db.Set<NakitBagisHareket>()
                .AsNoTracking()
                .Where(x => x.BagisTarihi != null && x.BagisTarihi.Value >= startDate)
                .ToListAsync(cancellationToken);
        }

        public Task<NakitBagisHareket?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => _unitOfWork.NakitBagisHareket.GetByIdAsync(id, cancellationToken);

        public async Task<bool> AddAsync(NakitBagisHareket model, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.NakitBagisHareket.AddAsync(model, cancellationToken);
            await _unitOfWork.NakitBagisHareket.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> UpdateAsync(NakitBagisHareket model, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.NakitBagisHareket.UpdateAsync(model, null, cancellationToken);
            await _unitOfWork.NakitBagisHareket.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAsync(NakitBagisHareket model, CancellationToken cancellationToken = default)
        {
            _unitOfWork.NakitBagisHareket.Remove(model);
            await _unitOfWork.NakitBagisHareket.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
