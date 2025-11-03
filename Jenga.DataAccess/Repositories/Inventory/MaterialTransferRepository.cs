using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.Inventory;
using Jenga.Models.Inventory;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.Inventory
{
    public class MaterialTransferRepository : Repository<MaterialTransfer>, IMaterialTransferRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        public MaterialTransferRepository(IDbContextFactory<ApplicationDbContext> dbFactory) : base(dbFactory)
        {
            _dbFactory = dbFactory;
        }
        // Ekstra metotlar eklenebilir.
    }
}