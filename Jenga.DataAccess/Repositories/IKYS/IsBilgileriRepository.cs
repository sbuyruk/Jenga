using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.IKYS;
using Jenga.Models.IKYS;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.IKYS;

public class IsBilgileriRepository : Repository<IsBilgileri>, IIsBilgileriRepository
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public IsBilgileriRepository(IDbContextFactory<ApplicationDbContext> dbFactory) : base(dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public new async Task<List<IsBilgileri>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var db = _dbFactory.CreateDbContext();
        return await db.Set<IsBilgileri>()
            .AsNoTracking()
            .Include(ib => ib.UnvanTanim)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        await using var db = _dbFactory.CreateDbContext();
        await db.SaveChangesAsync(cancellationToken);
    }
}
