using Jenga.DataAccess.Data;
using Jenga.Models.Inventory;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.Inventory
{
    public class MaterialModelService : IMaterialModelService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public MaterialModelService(IDbContextFactory<ApplicationDbContext> dbFactory)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        }

        public async Task<List<MaterialModel>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.MaterialModel_Table.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<MaterialModel?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.MaterialModel_Table.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        }

        public async Task<bool> AddAsync(MaterialModel model, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            await db.MaterialModel_Table.AddAsync(model, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> UpdateAsync(MaterialModel model, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            db.MaterialModel_Table.Update(model);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAsync(MaterialModel model, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var entity = await db.MaterialModel_Table.FirstOrDefaultAsync(m => m.Id == model.Id, cancellationToken);
            if (entity == null) return false;
            db.MaterialModel_Table.Remove(entity);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}