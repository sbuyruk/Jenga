using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.Ortak;
using Jenga.Models.Ortak;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.Ortak
{
    public class IlceRepository : Repository<Ilce>, IIlceRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        public IlceRepository(IDbContextFactory<ApplicationDbContext> dbFactory) : base(dbFactory)
        {
            _dbFactory = dbFactory;
        }

        // Async Save örneği (factory ile kısa ömürlü context kullanıldığında sync Save() anlamsızdır)
        public async Task SaveAsync(CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            await db.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<Ilce>> GetByIlIdAsync(int ilId, CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            return await db.Ilce_Table
                .AsNoTracking()
                .Where(x => x.IlId == ilId)
                .ToListAsync(cancellationToken);
        }
    }
}