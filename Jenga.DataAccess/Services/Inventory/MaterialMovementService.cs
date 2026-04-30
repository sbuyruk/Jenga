using Jenga.DataAccess.Data;
using Jenga.Models.Inventory;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.Inventory
{
    public class MaterialMovementService : IMaterialMovementService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public MaterialMovementService(IDbContextFactory<ApplicationDbContext> dbFactory)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        }

        public async Task AddAsync(MaterialMovement movement, CancellationToken cancellationToken = default)
        {
            if (movement == null) throw new ArgumentNullException(nameof(movement));

            // Ensure MovementDate and a sensible Operation value if caller didn't set them.
            if (movement.MovementDate == default) movement.MovementDate = DateTime.Now;

            movement.Operation ??= !string.IsNullOrWhiteSpace(movement.MovementType)
                ? movement.MovementType
                : (movement.FromLocationId.HasValue && movement.ToLocationId.HasValue ? "Transfer"
                    : (!movement.FromLocationId.HasValue && movement.ToLocationId.HasValue ? "Giriş"
                        : (movement.FromLocationId.HasValue && !movement.ToLocationId.HasValue ? "Çıkış" : "Diğer")));

            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            await db.MaterialMovement_Table.AddAsync(movement, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<MaterialMovement>> GetMovementsByMaterialIdAsync(int materialId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.MaterialMovement_Table
                .AsNoTracking()
                .Where(x => x.MaterialId == materialId)
                .ToListAsync(cancellationToken);
        }
    }
}