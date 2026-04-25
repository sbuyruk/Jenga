using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.TBYS;
using Jenga.Models.TBYS;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.TBYS
{
    public class TasinmazTaahhutRepository : ITasinmazTaahhutRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public TasinmazTaahhutRepository(IDbContextFactory<ApplicationDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<TasinmazTaahhut?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            return await db.TasinmazTaahhut_Table
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<List<TasinmazTaahhut>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            return await db.TasinmazTaahhut_Table
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<List<TasinmazTaahhut>> GetByTasinmazIdAsync(int tasinmazId, CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            return await db.TasinmazTaahhut_Table
                .AsNoTracking()
                .Where(x => x.TasinmazId == tasinmazId)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<TasinmazTaahhut>> GetByBagisciIdAsync(int bagisciId, CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            return await db.TasinmazTaahhut_Table
                .AsNoTracking()
                .Where(x => x.BagisciId == bagisciId)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(TasinmazTaahhut entity, CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            await db.TasinmazTaahhut_Table.AddAsync(entity, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(TasinmazTaahhut entity, CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            db.TasinmazTaahhut_Table.Update(entity);
            await db.SaveChangesAsync(cancellationToken);
        }

        public void Remove(TasinmazTaahhut entity)
        {
            using var db = _dbFactory.CreateDbContext();
            db.TasinmazTaahhut_Table.Remove(entity);
            db.SaveChanges();
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            return await db.SaveChangesAsync(cancellationToken);
        }
    }
}
