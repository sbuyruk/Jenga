using Jenga.DataAccess.Data;
using Jenga.Models.FTK;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.FTK
{
    public class FtkKisiService : IFtkKisiService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public FtkKisiService(IDbContextFactory<ApplicationDbContext> dbFactory)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        }

        public async Task<List<FtkKisi>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.FTKKisi_Table.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<FtkKisi?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.FTKKisi_Table.AsNoTracking().FirstOrDefaultAsync(k => k.Id == id, cancellationToken);
        }

        public async Task<bool> AddAsync(FtkKisi model, CancellationToken cancellationToken = default)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            await db.FTKKisi_Table.AddAsync(model, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> UpdateAsync(FtkKisi model, CancellationToken cancellationToken = default)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            db.FTKKisi_Table.Update(model);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAsync(FtkKisi model, CancellationToken cancellationToken = default)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            db.FTKKisi_Table.Remove(model);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
