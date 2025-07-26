using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.TBYS;
using Jenga.Models.TBYS;

namespace Jenga.DataAccess.Repositories.TBYS
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
