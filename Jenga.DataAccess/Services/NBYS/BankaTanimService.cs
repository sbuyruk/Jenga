using Jenga.DataAccess.Data;
using Jenga.Models.NBYS;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.NBYS
{
    public class BankaTanimService : IBankaTanimService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public BankaTanimService(IDbContextFactory<ApplicationDbContext> dbFactory)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        }

        public async Task<List<BankaTanim>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.BankaTanim_Table.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<BankaTanim?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.BankaTanim_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<bool> AddAsync(BankaTanim model, CancellationToken cancellationToken = default)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            await db.BankaTanim_Table.AddAsync(model, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> UpdateAsync(BankaTanim model, CancellationToken cancellationToken = default)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            db.BankaTanim_Table.Update(model);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAsync(BankaTanim model, CancellationToken cancellationToken = default)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            db.BankaTanim_Table.Remove(model);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
