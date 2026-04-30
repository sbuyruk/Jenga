using Jenga.DataAccess.Data;
using Jenga.Models.Inventory;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.Inventory
{
    public class MaterialCategoryService : IMaterialCategoryService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public MaterialCategoryService(IDbContextFactory<ApplicationDbContext> dbFactory)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        }

        public async Task<List<MaterialCategory>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.MaterialCategory_Table.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<MaterialCategory?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.MaterialCategory_Table.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        }

        public async Task<bool> AddAsync(MaterialCategory category, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            await db.MaterialCategory_Table.AddAsync(category, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> UpdateAsync(MaterialCategory category, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            db.MaterialCategory_Table.Update(category);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAsync(int categoryId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

            if (await db.MaterialCategory_Table.AsNoTracking().AnyAsync(m => m.ParentCategoryId == categoryId, cancellationToken))
                return false;

            var entity = await db.MaterialCategory_Table.FirstOrDefaultAsync(m => m.Id == categoryId, cancellationToken);
            if (entity != null)
            {
                db.MaterialCategory_Table.Remove(entity);
                await db.SaveChangesAsync(cancellationToken);
                return true;
            }
            return false;
        }

        public async Task<bool> AnyAsync(Expression<Func<MaterialCategory, bool>> predicate, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.MaterialCategory_Table.AsNoTracking().AnyAsync(predicate, cancellationToken);
        }

        public async Task<(bool CanDelete, string? Reason)> CanDeleteAsync(int id)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();

            if (await db.MaterialCategory_Table.AsNoTracking().AnyAsync(m => m.ParentCategoryId == id))
                return (false, "Bu kategori bir malzemenin üst kategorisi olarak kullanılıyor, önce onu silmelisiniz.");
            if (await db.Material_Table.AsNoTracking().AnyAsync(m => m.CategoryId == id))
                return (false, "Bu kategori bir malzemenin kategorisi kullanılıyor, önce onu silmelisiniz.");

            return (true, null);
        }
    }
}