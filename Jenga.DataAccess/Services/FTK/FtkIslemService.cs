using Jenga.DataAccess.Data;
using Jenga.Models.FTK;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace Jenga.DataAccess.Services.FTK
{
    public class FtkIslemService : IFtkIslemService
    {
        private const string Source = nameof(FtkIslemService);

        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService _logService;

        public FtkIslemService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        }

        public async Task<Result<List<FtkIslem>>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.FTKIslem_Table.AsNoTracking().ToListAsync(cancellationToken);
                return Result.Success(list);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.GetAllAsync");
                return Result.Failure<List<FtkIslem>>(Error.Unexpected("FTK işlem listesi alınamadı.", ex, "FtkIslem.GetAll.Failed"));
            }
        }

        public async Task<Result<List<FtkIslemDashboardItem>>> GetForBolgeDashboardAsync(IEnumerable<int> ftkIslemIds, CancellationToken cancellationToken = default)
        {
            try
            {
                var idsList = ftkIslemIds?.ToList() ?? [];
                if (idsList.Count == 0)
                    return Result.Success(new List<FtkIslemDashboardItem>());

                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.FTKIslem_Table.AsNoTracking()
                    .Where(i => idsList.Contains(i.Id))
                    .Select(i => new FtkIslemDashboardItem
                    {
                        Id = i.Id,
                        KurulusTarihi = i.KurulusTarihi,
                        GuncellemeTarihi = i.GuncellemeTarihi
                    })
                    .ToListAsync(cancellationToken);
                return Result.Success(list);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.GetForBolgeDashboardAsync");
                return Result.Failure<List<FtkIslemDashboardItem>>(Error.Unexpected("Bölge FTK işlem listesi alınamadı.", ex, "FtkIslem.GetForBolgeDashboard.Failed"));
            }
        }

        public async Task<Result<FtkIslem>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.FTKIslem_Table.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
                if (entity is null)
                    return Result.Failure<FtkIslem>(Error.NotFound($"FTK işlem bulunamadı (Id={id}).", "FtkIslem.NotFound"));
                return Result.Success(entity);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.GetByIdAsync");
                return Result.Failure<FtkIslem>(Error.Unexpected("FTK işlem getirilemedi.", ex, "FtkIslem.GetById.Failed"));
            }
        }

        public async Task<Result> AddAsync(FtkIslem model, CancellationToken cancellationToken = default)
        {
            if (model == null)
                return Result.Failure(Error.Validation("FTK işlem boş olamaz.", "FtkIslem.Null"));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                await db.FTKIslem_Table.AddAsync(model, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.AddAsync");
                return Result.Failure(Error.Unexpected("FTK işlem eklenemedi.", ex, "FtkIslem.Add.Failed"));
            }
        }

        public async Task<Result> UpdateAsync(FtkIslem model, CancellationToken cancellationToken = default)
        {
            if (model == null)
                return Result.Failure(Error.Validation("FTK işlem boş olamaz.", "FtkIslem.Null"));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.FTKIslem_Table.Update(model);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.UpdateAsync");
                return Result.Failure(Error.Unexpected("FTK işlem güncellenemedi.", ex, "FtkIslem.Update.Failed"));
            }
        }

        public async Task<Result> DeleteAsync(FtkIslem model, CancellationToken cancellationToken = default)
        {
            if (model == null)
                return Result.Failure(Error.Validation("FTK işlem boş olamaz.", "FtkIslem.Null"));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.FTKIslem_Table.Remove(model);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.DeleteAsync");
                return Result.Failure(Error.Unexpected("FTK işlem silinemedi.", ex, "FtkIslem.Delete.Failed"));
            }
        }
    }
}
