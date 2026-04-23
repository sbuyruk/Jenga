using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.FTK;
using Jenga.Models.FTK;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.FTK
{
    public class FtkRepository : Repository<Ftk>, IFtkRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public FtkRepository(IDbContextFactory<ApplicationDbContext> dbFactory) : base(dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<List<Ftk>> GetLatestPerIslemAsync(CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            var ftkSet = db.Set<Ftk>().AsNoTracking();

            // WHERE Sayac = (SELECT MAX(Sayac) FROM FTK_Table WHERE FTKIslemId = A.FTKIslemId)
            return await (
                from f in ftkSet
                where f.FtkIslemId != null
                   && f.Sayac == ftkSet
                        .Where(x => x.FtkIslemId == f.FtkIslemId)
                        .Max(x => x.Sayac)
                select f
            ).ToListAsync(cancellationToken);
        }
    }
}
