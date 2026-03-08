using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.TBYS;
using Jenga.Models.TBYS;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Jenga.DataAccess.Repositories.TBYS
{
    public class OdemeRepository : Repository<Odeme>, IOdemeRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public OdemeRepository(IDbContextFactory<ApplicationDbContext> dbFactory) : base(dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<Odeme?> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            return await db.Odeme_Table
                // include related entities if navigation properties exist, e.g.:
                // .Include(o => o.Kiraci)
                // .Include(o => o.Sozlesme)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
        }

        public async Task<List<Odeme>> GetBySozlesmeIdAsync(int sozlesmeId, CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            return await db.Odeme_Table
                .AsNoTracking()
                .Where(o => o.SozlesmeId == sozlesmeId)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Odeme>> GetByKiraciIdAsync(int kiraciId, CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            return await db.Odeme_Table
                .AsNoTracking()
                .Where(o => o.KiraciId == kiraciId)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Odeme>> GetByOdemePlaniIdAsync(int odemePlaniId, CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            return await db.Odeme_Table
                .AsNoTracking()
                .Where(o => o.OdemePlaniId == odemePlaniId)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Odeme>> GetAllWithOdemePlaniAsync(CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();

            // Inner join at DB level: return Odeme rows that have a matching OdemePlani (Odeme.OdemePlaniId == OdemePlani.Id)
            return await db.Odeme_Table
                .Where(o => o.OdemePlaniId != null)
                .Join(db.OdemePlani_Table,
                      o => o.OdemePlaniId,
                      p => p.Id,
                      (o, p) => o)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
    }
}
