using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.NBYS;
using Jenga.Models.NBYS;

namespace Jenga.DataAccess.Repositories.NBYS
{
    public class NakitBagisHareketRepository : Repository<NakitBagisHareket>, INakitBagisHareketRepository
    {
        ApplicationDbContext _db;
        public NakitBagisHareketRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }


    }
}
