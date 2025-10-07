using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.Inventory;
using Jenga.Models.Inventory;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.Inventory
{
    public class MaterialUnitRepository : Repository<MaterialUnit>, IMaterialUnitRepository
    {
        private readonly ApplicationDbContext _db;
        public MaterialUnitRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

    }
}