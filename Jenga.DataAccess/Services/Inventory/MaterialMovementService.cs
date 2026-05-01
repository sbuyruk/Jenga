using Jenga.DataAccess.Data;
using Jenga.Models.Inventory;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.Inventory
{
    public class MaterialMovementService : IMaterialMovementService
    {
        private const string Source = nameof(MaterialMovementService);

        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService _logService;

        public MaterialMovementService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService;
        }

        public async Task<Result> AddAsync(MaterialMovement movement, CancellationToken cancellationToken = default)
        {
            if (movement == null)
                return Result.Failure(Error.Validation("Hareket boş olamaz.", "MaterialMovement.Null"));

            // Ensure MovementDate and a sensible Operation value if caller didn't set them.
            if (movement.MovementDate == default) movement.MovementDate = DateTime.Now;

            movement.Operation ??= !string.IsNullOrWhiteSpace(movement.MovementType)
                ? movement.MovementType
                : (movement.FromLocationId.HasValue && movement.ToLocationId.HasValue ? "Transfer"
                    : (!movement.FromLocationId.HasValue && movement.ToLocationId.HasValue ? "Giriş"
                        : (movement.FromLocationId.HasValue && !movement.ToLocationId.HasValue ? "Çıkış" : "Diğer")));

            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                await db.MaterialMovement_Table.AddAsync(movement, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService?.LogException(ex, $"{Source}.AddAsync");
                return Result.Failure(Error.Unexpected("Hareket eklenemedi.", ex, "MaterialMovement.Add.Failed"));
            }
        }

        public async Task<Result<List<MaterialMovement>>> GetMovementsByMaterialIdAsync(int materialId, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.MaterialMovement_Table
                    .AsNoTracking()
                    .Where(x => x.MaterialId == materialId)
                    .ToListAsync(cancellationToken);
                return Result.Success(list);
            }
            catch (Exception ex)
            {
                _logService?.LogException(ex, $"{Source}.GetMovementsByMaterialIdAsync");
                return Result.Failure<List<MaterialMovement>>(Error.Unexpected("Hareket listesi alınamadı.", ex, "MaterialMovement.GetByMaterial.Failed"));
            }
        }
    }
}