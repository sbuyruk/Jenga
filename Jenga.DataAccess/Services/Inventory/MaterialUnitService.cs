using Jenga.DataAccess.Data;
using Jenga.Models.Inventory;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.Inventory
{
    public class MaterialUnitService : IMaterialUnitService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private List<MaterialUnit>? _unitsCache;

        public MaterialUnitService(IDbContextFactory<ApplicationDbContext> dbFactory)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        }

        public async Task<List<MaterialUnit>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.MaterialUnit_Table.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<MaterialUnit?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.MaterialUnit_Table.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        }

        public async Task<bool> AddAsync(MaterialUnit unit, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            await db.MaterialUnit_Table.AddAsync(unit, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            _unitsCache = null;
            return true;
        }

        public async Task<bool> UpdateAsync(MaterialUnit unit, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            db.MaterialUnit_Table.Update(unit);
            await db.SaveChangesAsync(cancellationToken);
            _unitsCache = null;
            return true;
        }

        public async Task<bool> DeleteAsync(MaterialUnit unit, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var entity = await db.MaterialUnit_Table.FirstOrDefaultAsync(u => u.Id == unit.Id, cancellationToken);
            if (entity == null) return false;
            db.MaterialUnit_Table.Remove(entity);
            await db.SaveChangesAsync(cancellationToken);
            _unitsCache = null;
            return true;
        }

        // Yardımcı Metot
        public async Task<string> GetUnitSymbolAsync(int unitId, CancellationToken cancellationToken = default)
        {
            if (_unitsCache == null)
                _unitsCache = await GetAllAsync(cancellationToken);
            var unit = _unitsCache.FirstOrDefault(x => x.Id == unitId);
            return unit?.Symbol ?? "";
        }
    }
}