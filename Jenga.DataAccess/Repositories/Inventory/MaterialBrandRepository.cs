using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.Inventory;
using Jenga.Models.Inventory;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.Inventory
{
    public class MaterialBrandRepository : Repository<MaterialBrand>, IMaterialBrandRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        public MaterialBrandRepository(IDbContextFactory<ApplicationDbContext> dbFactory) : base(dbFactory)
        {
            _dbFactory = dbFactory;
        }

        // Örnek: marka adına göre arama
        public async Task<MaterialBrand?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            return await db.MaterialBrand_Table.AsNoTracking().FirstOrDefaultAsync(b => b.BrandName == name, cancellationToken);
        }
    }
}