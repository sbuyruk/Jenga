using Jenga.DataAccess.Data;
using Jenga.Models.Inventory;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.Inventory
{
    public class MaterialInventoryService : IMaterialInventoryService
    {
        private const string Source = nameof(MaterialInventoryService);

        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService _logService;

        public MaterialInventoryService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        }

        public async Task<Result<List<MaterialInventory>>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.MaterialInventory_Table.AsNoTracking().ToListAsync(cancellationToken);
                return Result.Success(list);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.GetAllAsync");
                return Result.Failure<List<MaterialInventory>>(Error.Unexpected("Envanter listesi alınamadı.", ex, "MaterialInventory.GetAll.Failed"));
            }
        }

        public async Task<Result<bool>> AnyAsync(Expression<Func<MaterialInventory, bool>> predicate)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync();
                var any = await db.MaterialInventory_Table.AsNoTracking().AnyAsync(predicate);
                return Result.Success(any);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.AnyAsync");
                return Result.Failure<bool>(Error.Unexpected("Envanter sorgusu yapılamadı.", ex, "MaterialInventory.Any.Failed"));
            }
        }
    }
}
