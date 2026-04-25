using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.IKYS;
using Jenga.Models.IKYS;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.IKYS;

public class AileRepository : Repository<Aile>, IAileRepository
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public AileRepository(IDbContextFactory<ApplicationDbContext> dbFactory) : base(dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        await using var db = _dbFactory.CreateDbContext();
        await db.SaveChangesAsync(cancellationToken);
    }
}
