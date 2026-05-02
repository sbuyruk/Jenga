using Jenga.DataAccess.Data;
using Jenga.Models.TBYS;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.TBYS;

public class YasalFaizService : IYasalFaizService
{
    private const string Source = nameof(YasalFaizService);

    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly ILogService _logService;

    public YasalFaizService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
    {
        _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        _logService = logService ?? throw new ArgumentNullException(nameof(logService));
    }

    public async Task<Result<List<YasalFaiz>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var list = await db.YasalFaiz_Table.AsNoTracking().ToListAsync(cancellationToken);
            return Result<List<YasalFaiz>>.Success(list);
        }
        catch (Exception ex)
        {
            _logService.LogError($"{Source}.GetAllAsync hata.", ex);
            return Result<List<YasalFaiz>>.Failure(Error.Unexpected("Yasal faiz listesi alınamadı.", ex));
        }
    }

    public async Task<Result<YasalFaiz>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var entity = await db.YasalFaiz_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            return entity is null
                ? Result<YasalFaiz>.Failure(Error.NotFound("Yasal faiz bulunamadı."))
                : Result<YasalFaiz>.Success(entity);
        }
        catch (Exception ex)
        {
            _logService.LogError($"{Source}.GetByIdAsync hata.", ex);
            return Result<YasalFaiz>.Failure(Error.Unexpected("Yasal faiz alınamadı.", ex));
        }
    }

    public async Task<Result> AddAsync(YasalFaiz yasalFaiz, CancellationToken cancellationToken = default)
    {
        if (yasalFaiz is null)
            return Result.Failure(Error.Validation("Yasal faiz kaydı boş olamaz."));

        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            await db.YasalFaiz_Table.AddAsync(yasalFaiz, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogError($"{Source}.AddAsync hata.", ex);
            return Result.Failure(Error.Unexpected("Yasal faiz eklenemedi.", ex));
        }
    }

    public async Task<Result> UpdateAsync(YasalFaiz yasalFaiz, CancellationToken cancellationToken = default)
    {
        if (yasalFaiz is null)
            return Result.Failure(Error.Validation("Yasal faiz kaydı boş olamaz."));

        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            db.YasalFaiz_Table.Update(yasalFaiz);
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogError($"{Source}.UpdateAsync hata.", ex);
            return Result.Failure(Error.Unexpected("Yasal faiz güncellenemedi.", ex));
        }
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var entity = await db.YasalFaiz_Table.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (entity is null)
                return Result.Failure(Error.NotFound("Silinecek yasal faiz bulunamadı."));

            db.YasalFaiz_Table.Remove(entity);
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogError($"{Source}.DeleteAsync hata.", ex);
            return Result.Failure(Error.Unexpected("Yasal faiz silinemedi.", ex));
        }
    }

    public async Task<Result<bool>> AnyAsync(Expression<Func<YasalFaiz, bool>> predicate, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var any = await db.YasalFaiz_Table.AnyAsync(predicate, cancellationToken);
            return Result<bool>.Success(any);
        }
        catch (Exception ex)
        {
            _logService.LogError($"{Source}.AnyAsync hata.", ex);
            return Result<bool>.Failure(Error.Unexpected("Yasal faiz sorgulanamadı.", ex));
        }
    }
}
