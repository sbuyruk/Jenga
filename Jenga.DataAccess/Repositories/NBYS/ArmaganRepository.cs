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
    public class ArmaganRepository : Repository<Armagan>, IArmaganRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public ArmaganRepository(IDbContextFactory<ApplicationDbContext> dbFactory) : base(dbFactory)
        {
            _dbFactory = dbFactory;
        }
    }
}
