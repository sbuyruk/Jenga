using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.Inventory;
using Jenga.Models.Inventory;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.Inventory
{
    public class MaterialExitRepository : Repository<MaterialExit>, IMaterialExitRepository
    {
        private readonly ApplicationDbContext _db;
        public MaterialExitRepository(ApplicationDbContext db) : base(db) { _db = db; }

    }
}