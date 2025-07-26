using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.NBYS;
using Jenga.Models.NBYS;

namespace Jenga.DataAccess.Repositories.NBYS
{
    public class NakitBagisciRepository : Repository<NakitBagisci>, INakitBagisciRepository
    {
        ApplicationDbContext _db;
        public NakitBagisciRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }

    }
}
