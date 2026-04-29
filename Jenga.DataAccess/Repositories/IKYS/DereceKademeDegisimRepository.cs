using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.IKYS;
using Jenga.Models.IKYS;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.IKYS;

public class DereceKademeDegisimRepository : Repository<DereceKademeDegisim>, IDereceKademeDegisimRepository
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public DereceKademeDegisimRepository(IDbContextFactory<ApplicationDbContext> dbFactory) : base(dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<List<DereceKademeDegisim>> GetDereceYukseltmeAsync(CancellationToken cancellationToken)
    {
        await using var db = _dbFactory.CreateDbContext();
        return await db.DereceKademeDegisim_Table
            .Where(x => x.Degisim == "Derece Yükseltme")
            .ToListAsync(cancellationToken);
    }

    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        await using var db = _dbFactory.CreateDbContext();
        await db.SaveChangesAsync(cancellationToken);
    }
}
