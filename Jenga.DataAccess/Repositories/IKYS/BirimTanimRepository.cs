using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.IKYS;
using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Repositories.IKYS
{
    public class BirimTanimRepository : Repository<BirimTanim>, IBirimTanimRepository
    {
        ApplicationDbContext _db;
        public BirimTanimRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }

    }
}
