using Jenga.DataAccess.Data;
using Jenga.Models.NBYS;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.NBYS
{
    public class DuzenliNakitBagisciService : IDuzenliNakitBagisciService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public DuzenliNakitBagisciService(IDbContextFactory<ApplicationDbContext> dbFactory)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        }

        public async Task<List<DuzenliNakitBagisci>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.DuzenliNakitBagisci_Table.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<DuzenliNakitBagisci?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.DuzenliNakitBagisci_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<bool> AddAsync(DuzenliNakitBagisci model, CancellationToken cancellationToken = default)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            await db.DuzenliNakitBagisci_Table.AddAsync(model, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> UpdateAsync(DuzenliNakitBagisci model, CancellationToken cancellationToken = default)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            db.DuzenliNakitBagisci_Table.Update(model);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAsync(DuzenliNakitBagisci model, CancellationToken cancellationToken = default)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            db.DuzenliNakitBagisci_Table.Remove(model);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
