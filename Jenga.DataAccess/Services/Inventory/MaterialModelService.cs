using Jenga.DataAccess.Data;
using Jenga.Models.Inventory;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.Inventory
{
    public class MaterialModelService : IMaterialModelService
    {
        private const string Source = nameof(MaterialModelService);

        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService _logService;

        public MaterialModelService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        }

        public async Task<Result<List<MaterialModel>>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.MaterialModel_Table.AsNoTracking().ToListAsync(cancellationToken);
                return Result.Success(list);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.GetAllAsync");
                return Result.Failure<List<MaterialModel>>(Error.Unexpected("Model listesi alinamadi.", ex, "MaterialModel.GetAll.Failed"));
            }
        }

        public async Task<Result<MaterialModel>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.MaterialModel_Table.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
                if (entity is null)
                    return Result.Failure<MaterialModel>(Error.NotFound($"Model bulunamadi (Id={id}).", "MaterialModel.NotFound"));
                return Result.Success(entity);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.GetByIdAsync");
                return Result.Failure<MaterialModel>(Error.Unexpected("Model getirilemedi.", ex, "MaterialModel.GetById.Failed"));
            }
        }

        public async Task<Result> AddAsync(MaterialModel model, CancellationToken cancellationToken = default)
        {
            if (model == null)
                return Result.Failure(Error.Validation("Model bos olamaz.", "MaterialModel.Null"));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                await db.MaterialModel_Table.AddAsync(model, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.AddAsync");
                return Result.Failure(Error.Unexpected("Model eklenemedi.", ex, "MaterialModel.Add.Failed"));
            }
        }

        public async Task<Result> UpdateAsync(MaterialModel model, CancellationToken cancellationToken = default)
        {
            if (model == null)
                return Result.Failure(Error.Validation("Model bos olamaz.", "MaterialModel.Null"));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.MaterialModel_Table.Update(model);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.UpdateAsync");
                return Result.Failure(Error.Unexpected("Model güncellenemedi.", ex, "MaterialModel.Update.Failed"));
            }
        }

        public async Task<Result> DeleteAsync(MaterialModel model, CancellationToken cancellationToken = default)
        {
            if (model == null)
                return Result.Failure(Error.Validation("Model bos olamaz.", "MaterialModel.Null"));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.MaterialModel_Table.FirstOrDefaultAsync(m => m.Id == model.Id, cancellationToken);
                if (entity == null)
                    return Result.Failure(Error.NotFound($"Model bulunamadi (Id={model.Id}).", "MaterialModel.NotFound"));
                db.MaterialModel_Table.Remove(entity);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.DeleteAsync");
                return Result.Failure(Error.Unexpected("Model silinemedi.", ex, "MaterialModel.Delete.Failed"));
            }
        }
    }
}