using Jenga.DataAccess.Data;
using Jenga.Models.FTK;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.FTK
{
    public class FtkIslemService : IFtkIslemService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public FtkIslemService(IDbContextFactory<ApplicationDbContext> dbFactory)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        }

        public async Task<List<FtkIslem>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.FTKIslem_Table.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<FtkIslem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.FTKIslem_Table.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
        }

        public async Task<bool> AddAsync(FtkIslem model, CancellationToken cancellationToken = default)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            await db.FTKIslem_Table.AddAsync(model, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> UpdateAsync(FtkIslem model, CancellationToken cancellationToken = default)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            db.FTKIslem_Table.Update(model);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAsync(FtkIslem model, CancellationToken cancellationToken = default)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            db.FTKIslem_Table.Remove(model);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
