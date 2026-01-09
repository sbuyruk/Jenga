using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.TBYS;
using Jenga.Models.TBYS;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Repositories.TBYS
{
    public class OdemePlaniRepository : Repository<OdemePlani>, IOdemePlaniRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public OdemePlaniRepository(IDbContextFactory<ApplicationDbContext> dbFactory) : base(dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<bool> AnyAsync(Expression<Func<OdemePlani, bool>> predicate, CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            return await db.OdemePlani_Table.AnyAsync(predicate, cancellationToken);
        }

        // OdemePlani-specific queries can be added here.
    }
}
