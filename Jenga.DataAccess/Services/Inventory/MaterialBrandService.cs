using Jenga.DataAccess.Data;
using Jenga.Models.Inventory;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.Inventory
{
    public class MaterialBrandService : IMaterialBrandService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public MaterialBrandService(IDbContextFactory<ApplicationDbContext> dbFactory)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        }

        public async Task<List<MaterialBrand>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.MaterialBrand_Table.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<MaterialBrand?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.MaterialBrand_Table.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        }

        public async Task<bool> AddAsync(MaterialBrand brand, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            await db.MaterialBrand_Table.AddAsync(brand, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> UpdateAsync(MaterialBrand brand, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            db.MaterialBrand_Table.Update(brand);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAsync(MaterialBrand brand, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var entity = await db.MaterialBrand_Table.FirstOrDefaultAsync(b => b.Id == brand.Id, cancellationToken);
            if (entity == null) return false;
            db.MaterialBrand_Table.Remove(entity);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> AnyAsync(Expression<Func<MaterialBrand, bool>> predicate, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.MaterialBrand_Table.AsNoTracking().AnyAsync(predicate, cancellationToken);
        }
    }
}