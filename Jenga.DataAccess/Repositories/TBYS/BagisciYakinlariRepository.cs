using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.TBYS;
using Jenga.Models.TBYS;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.TBYS
{
    public class BagisciYakinlariRepository : Repository<BagisciYakinlari>, IBagisciYakinlariRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public BagisciYakinlariRepository(IDbContextFactory<ApplicationDbContext> dbFactory) : base(dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<List<BagisciYakinlari>> GetByBagisciIdAsync(long bagisciId, CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            return await db.BagisciYakinlari_Table
                .AsNoTracking()
                .Where(x => x.BagisciId == bagisciId)
                .OrderBy(x => x.Sira)
                .ToListAsync(cancellationToken);
        }
    }
}
