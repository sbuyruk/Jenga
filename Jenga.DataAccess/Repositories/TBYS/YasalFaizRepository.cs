using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.TBYS;
using Jenga.Models.TBYS;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Repositories.TBYS;

public class YasalFaizRepository : Repository<YasalFaiz>, IYasalFaizRepository
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public YasalFaizRepository(IDbContextFactory<ApplicationDbContext> dbFactory) : base(dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<bool> AnyAsync(Expression<Func<YasalFaiz, bool>> predicate, CancellationToken cancellationToken = default)
    {
        await using var db = _dbFactory.CreateDbContext();
        return await db.YasalFaiz_Table.AnyAsync(predicate, cancellationToken);
    }
}