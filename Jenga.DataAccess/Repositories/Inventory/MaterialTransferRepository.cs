using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.Inventory;
using Jenga.Models.Inventory;

namespace Jenga.DataAccess.Repositories.Inventory
{
    public class MaterialTransferRepository : Repository<MaterialTransfer>, IMaterialTransferRepository
    {
        private readonly ApplicationDbContext _db;
        public MaterialTransferRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        // Ekstra metotlar eklenebilir.
    }
}