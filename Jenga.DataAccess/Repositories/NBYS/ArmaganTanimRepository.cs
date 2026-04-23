using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.NBYS;
using Jenga.Models.NBYS;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.NBYS
{
    public class ArmaganTanimRepository : Repository<ArmaganTanim>, IArmaganTanimRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public ArmaganTanimRepository(IDbContextFactory<ApplicationDbContext> dbFactory) : base(dbFactory)
        {
            _dbFactory = dbFactory;
        }
    }
}
