using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.TBYS;
using Jenga.Models.TBYS;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.TBYS
{
    public class BagisciTalepleriRepository : Repository<BagisciTalepleri>, IBagisciTalepleriRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public BagisciTalepleriRepository(IDbContextFactory<ApplicationDbContext> dbFactory) : base(dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<List<BagisciTalepleri>> GetByBagisciIdAsync(long bagisciId, CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            return await db.BagisciTalepleri_Table
                .AsNoTracking()
                .Where(x => x.BagisciId == bagisciId)
                .ToListAsync(cancellationToken);
        }
    }
}
