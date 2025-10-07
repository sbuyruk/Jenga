using System.Linq.Expressions;
using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.Inventory;
using Jenga.Models.Inventory;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.Inventory
{
    public class MaterialEntryRepository : Repository<MaterialEntry>, IMaterialEntryRepository
    {
        private readonly ApplicationDbContext _db;
        public MaterialEntryRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<bool> AnyAsync(Expression<Func<MaterialEntry, bool>> predicate)
        {
            return await _db.MaterialEntry_Table.AnyAsync(predicate);
        }


    }
}