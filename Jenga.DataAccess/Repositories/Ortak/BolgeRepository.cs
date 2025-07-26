using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.Ortak;
using Jenga.Models.Ortak;

namespace Jenga.DataAccess.Repositories.Ortak
{
    public class BolgeRepository : Repository<Bolge>, IBolgeRepository
    {
        ApplicationDbContext _db;
        public BolgeRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }

    }
}
