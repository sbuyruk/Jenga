using Jenga.DataAccess.Data;
using Jenga.Models.FTK;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.FTK
{
    public class FtkService : IFtkService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public FtkService(IDbContextFactory<ApplicationDbContext> dbFactory)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        }

        public async Task<List<Ftk>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.FTK_Table.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<List<Ftk>> GetLatestPerIslemAsync(CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var ftkSet = db.FTK_Table.AsNoTracking();

            // WHERE Sayac = (SELECT MAX(Sayac) FROM FTK_Table WHERE FTKIslemId = A.FTKIslemId)
            return await (
                from f in ftkSet
                where f.FtkIslemId != null
                   && f.Sayac == ftkSet
                        .Where(x => x.FtkIslemId == f.FtkIslemId)
                        .Max(x => x.Sayac)
                select f
            ).ToListAsync(cancellationToken);
        }

        public async Task<Ftk?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.FTK_Table.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
        }

        public async Task<bool> AddAsync(Ftk model, CancellationToken cancellationToken = default)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            await db.FTK_Table.AddAsync(model, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> UpdateAsync(Ftk model, CancellationToken cancellationToken = default)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            db.FTK_Table.Update(model);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAsync(Ftk model, CancellationToken cancellationToken = default)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            db.FTK_Table.Remove(model);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
