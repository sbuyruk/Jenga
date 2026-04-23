using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.Common;
using Jenga.Models.Common;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.Common
{
    public class IlRepository : Repository<Il>, IIlRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        public IlRepository(IDbContextFactory<ApplicationDbContext> dbFactory) : base(dbFactory)
        {
            _dbFactory = dbFactory;
        }
        public async Task<List<Il>> GetAktifIllerAsync(CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            var ilSet = db.Set<Il>().AsNoTracking();

            // WHERE Sayac = (SELECT MAX(Sayac) FROM FTK_Table WHERE FTKIslemId = A.FTKIslemId)
            return await (
                from i in ilSet
                where i.IlAdi != null
                   //'Boş','Yok','---','Yurtdışı','Yok','Almanya',' ','Diğer'
                   && i.IlAdi != " "
                   && i.IlAdi != "Boş"
                   && i.IlAdi != "Yok"
                   && i.IlAdi != "---"
                   && i.IlAdi != "Yurtdışı"
                   && i.IlAdi != "Almanya"
                   && i.IlAdi != "Diğer"
                   && i.Aktif == true
                select i
            ).ToListAsync(cancellationToken);
        }
    }
}