using Jenga.DataAccess.Data;
using Jenga.Models.TBYS;
using Jenga.DataAccess.Repository.IRepository.TBYS;

namespace Jenga.DataAccess.Repository.TBYS
{
    public class TasinmazBagisciRepository : Repository<TasinmazBagisci>, ITasinmazBagisciRepository
    {
        ApplicationDbContext _db;
        public TasinmazBagisciRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }

    }
}
