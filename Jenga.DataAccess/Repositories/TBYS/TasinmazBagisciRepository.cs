using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.TBYS;
using Jenga.Models.TBYS;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.TBYS
{
    public class TasinmazBagisciRepository : Repository<TasinmazBagisci>, ITasinmazBagisciRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        public TasinmazBagisciRepository(IDbContextFactory<ApplicationDbContext> dbFactory) : base(dbFactory)
        {
            _dbFactory = dbFactory;
        }

        // If you need navigation properties, include them here
        public async Task<TasinmazBagisci?> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            return await db.TasinmazBagisci_Table
                //.Include(x => x.SomeNavigation)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        }
    }
}
