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
    public class BagisRepository : Repository<Bagis>, IBagisRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public BagisRepository(IDbContextFactory<ApplicationDbContext> dbFactory) : base(dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<Bagis?> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            return await db.Bagis_Table
                // include related entities if navigation properties exist, e.g.:
                // .Include(b => b.Bagisci)
                // .Include(b => b.Tasinmaz)
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        }

        public async Task<List<Bagis>> GetByBagisciIdAsync(int bagisciId, CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            return await db.Bagis_Table
                .AsNoTracking()
                .Where(b => b.BagisciId == bagisciId)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Bagis>> GetByTasinmazIdAsync(int tasinmazId, CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            return await db.Bagis_Table
                .AsNoTracking()
                .Where(b => b.TasinmazId == tasinmazId)
                .ToListAsync(cancellationToken);
        }
    }
}