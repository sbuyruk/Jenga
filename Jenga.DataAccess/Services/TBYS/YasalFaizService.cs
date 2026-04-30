using Jenga.DataAccess.Data;
using Jenga.Models.TBYS;
using Jenga.Utility.Logging;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.TBYS;

public class YasalFaizService : IYasalFaizService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly ILogService _logService;

    public YasalFaizService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
    {
        _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        _logService = logService;
    }

    public async Task<List<YasalFaiz>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.YasalFaiz_Table.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<YasalFaiz?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.YasalFaiz_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<bool> AddAsync(YasalFaiz yasalFaiz, CancellationToken cancellationToken = default)
    {
        if (yasalFaiz == null) throw new ArgumentNullException(nameof(yasalFaiz));
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            await db.YasalFaiz_Table.AddAsync(yasalFaiz, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logService?.LogError("Yasal faiz eklenirken hata oluştu.", ex);
            throw;
        }
    }

    public async Task<bool> UpdateAsync(YasalFaiz yasalFaiz, CancellationToken cancellationToken = default)
    {
        if (yasalFaiz == null) throw new ArgumentNullException(nameof(yasalFaiz));
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            db.YasalFaiz_Table.Update(yasalFaiz);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logService?.LogError("Yasal faiz güncellenirken hata oluştu.", ex);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var entity = await db.YasalFaiz_Table.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity == null) return false;

        db.YasalFaiz_Table.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> AnyAsync(Expression<Func<YasalFaiz, bool>> predicate, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.YasalFaiz_Table.AnyAsync(predicate, cancellationToken);
    }
}
