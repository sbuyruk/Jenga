using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.Inventory;
using Jenga.Models.Inventory;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.Inventory
{
    public class MaterialMovementRepository : Repository<MaterialMovement>, IMaterialMovementRepository
    {
        private readonly ApplicationDbContext _db;
        public MaterialMovementRepository(ApplicationDbContext db) : base(db) { _db = db; }

    }
}