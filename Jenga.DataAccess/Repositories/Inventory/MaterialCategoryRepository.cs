using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.Inventory;
using Jenga.Models.Inventory;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Repositories.Inventory
{
    public class MaterialCategoryRepository : Repository<MaterialCategory>, IMaterialCategoryRepository
    {
        private readonly ApplicationDbContext _db;
        public MaterialCategoryRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<bool> AnyAsync(Expression<Func<MaterialCategory, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _db.Set<MaterialCategory>().AnyAsync(predicate, cancellationToken);
        }
    }
}