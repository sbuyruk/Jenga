using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.Ortak;
using Jenga.Models.Ortak;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.Ortak
{
    public class BolgeRepository : Repository<Bolge>, IBolgeRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        public BolgeRepository(IDbContextFactory<ApplicationDbContext> dbFactory) : base(dbFactory)
        {
            _dbFactory = dbFactory;
        }

        // Örnek: bölgeye özel metot eklenecekse kısa ömürlü context ile kullanın
        public async Task<Bolge?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            return await db.Bolge_Table.AsNoTracking().FirstOrDefaultAsync(b => b.Adi == name, cancellationToken);
        }
    }
}