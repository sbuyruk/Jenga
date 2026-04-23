using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.FTK;
using Jenga.Models.FTK;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.FTK
{
    public class FtkIslemRepository : Repository<FtkIslem>, IFtkIslemRepository
    {
        public FtkIslemRepository(IDbContextFactory<ApplicationDbContext> dbFactory) : base(dbFactory)
        {
        }
    }
}
