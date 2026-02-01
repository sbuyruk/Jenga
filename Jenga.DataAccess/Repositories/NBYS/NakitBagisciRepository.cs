using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.NBYS;
using Jenga.Models.NBYS;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.NBYS
{
    public class NakitBagisciRepository : Repository<NakitBagisci>, INakitBagisciRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public NakitBagisciRepository(IDbContextFactory<ApplicationDbContext> dbFactory) : base(dbFactory)
        {
            _dbFactory = dbFactory;
        }
    }
}
