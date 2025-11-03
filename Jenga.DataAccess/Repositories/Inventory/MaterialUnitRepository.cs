using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.Inventory;
using Jenga.Models.Inventory;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.Inventory
{
    public class MaterialUnitRepository : Repository<MaterialUnit>, IMaterialUnitRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        public MaterialUnitRepository(IDbContextFactory<ApplicationDbContext> dbFactory) : base(dbFactory)
        {
            _dbFactory = dbFactory;
        }
    }
}