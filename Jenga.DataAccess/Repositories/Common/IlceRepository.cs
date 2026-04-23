using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.Common;
using Jenga.Models.Common;
using Jenga.Models.FTK;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.Common
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
        public async Task<List<Ilce>> GetAktifIlcelerAsync(CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            var ilceSet = db.Set<Ilce>().AsNoTracking();

            // WHERE Sayac = (SELECT MAX(Sayac) FROM FTK_Table WHERE FTKIslemId = A.FTKIslemId)
            return await (
                from i in ilceSet
                where i.IlceAdi != null
                   && i.IlceAdi != "Merkez"
                   && i.Aktif == true
                select i
            ).ToListAsync(cancellationToken);
        }
    }
}