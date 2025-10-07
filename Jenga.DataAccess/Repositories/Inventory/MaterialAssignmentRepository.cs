using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.Inventory;
using Jenga.Models.Inventory;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.Inventory
{
    public class MaterialAssignmentRepository : Repository<MaterialAssignment>, IMaterialAssignmentRepository
    {
        private readonly ApplicationDbContext _db;
        public MaterialAssignmentRepository(ApplicationDbContext db) : base(db) { _db = db; }


    }
}