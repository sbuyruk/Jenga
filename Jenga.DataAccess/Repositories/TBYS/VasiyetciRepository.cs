using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.TBYS;
using Jenga.Models.TBYS;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.TBYS
{
    public class VasiyetciRepository : Repository<Vasiyetci>, IVasiyetciRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public VasiyetciRepository(IDbContextFactory<ApplicationDbContext> dbFactory) : base(dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<List<Vasiyetci>> GetByTCKimlikAsync(long tcKimlik, CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            return await db.Vasiyetci_Table
                .AsNoTracking()
                .Where(x => x.TCKimlikNo == tcKimlik)
                .ToListAsync(cancellationToken);
        }
    }
}
