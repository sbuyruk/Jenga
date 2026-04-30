using Jenga.DataAccess.Data;
using Jenga.Models.NBYS;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.NBYS
{
    public class NakitBagisciService : INakitBagisciService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public NakitBagisciService(IDbContextFactory<ApplicationDbContext> dbFactory)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        }

        public async Task<List<NakitBagisci>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.NakitBagisci_Table.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<NakitBagisci?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.NakitBagisci_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<bool> AddAsync(NakitBagisci model, CancellationToken cancellationToken = default)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            await db.NakitBagisci_Table.AddAsync(model, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> UpdateAsync(NakitBagisci model, CancellationToken cancellationToken = default)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            db.NakitBagisci_Table.Update(model);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAsync(NakitBagisci model, CancellationToken cancellationToken = default)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            db.NakitBagisci_Table.Remove(model);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
