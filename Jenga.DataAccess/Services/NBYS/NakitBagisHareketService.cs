using Jenga.DataAccess.Data;
using Jenga.Models.NBYS;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.NBYS
{
    public class NakitBagisHareketService : INakitBagisHareketService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public NakitBagisHareketService(IDbContextFactory<ApplicationDbContext> dbFactory)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        }

        public async Task<List<NakitBagisHareket>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.NakitBagisHareket_Table.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<List<NakitBagisHareket>> GetLastYearsAsync(int years, CancellationToken cancellationToken = default)
        {
            var startDate = DateTime.Today.AddYears(-years);

            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.NakitBagisHareket_Table
                .AsNoTracking()
                .Where(x => x.BagisTarihi != null && x.BagisTarihi.Value >= startDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<NakitBagisHareket?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.NakitBagisHareket_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<bool> AddAsync(NakitBagisHareket model, CancellationToken cancellationToken = default)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            await db.NakitBagisHareket_Table.AddAsync(model, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> UpdateAsync(NakitBagisHareket model, CancellationToken cancellationToken = default)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            db.NakitBagisHareket_Table.Update(model);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAsync(NakitBagisHareket model, CancellationToken cancellationToken = default)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            db.NakitBagisHareket_Table.Remove(model);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
