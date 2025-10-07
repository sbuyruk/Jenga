using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.Inventory;
using Jenga.Models.Inventory;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.Inventory
{
    public class MaterialModelRepository : Repository<MaterialModel>, IMaterialModelRepository
    {
        private readonly ApplicationDbContext _db;
        public MaterialModelRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }


    }
}