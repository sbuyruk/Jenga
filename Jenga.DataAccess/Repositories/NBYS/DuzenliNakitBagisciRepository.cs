using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.NBYS;
using Jenga.Models.NBYS;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.NBYS
{
    public class DuzenliNakitBagisciRepository : Repository<DuzenliNakitBagisci>, IDuzenliNakitBagisciRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public DuzenliNakitBagisciRepository(IDbContextFactory<ApplicationDbContext> dbFactory) : base(dbFactory)
        {
            _dbFactory = dbFactory;
        }
    }
}
