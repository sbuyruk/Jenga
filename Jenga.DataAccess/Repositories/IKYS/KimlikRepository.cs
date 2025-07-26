using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.IKYS;
using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Repositories.IKYS
{
    public class KimlikRepository : Repository<Kimlik>, IKimlikRepository
    {
        ApplicationDbContext _db;
        public KimlikRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }

    }
}
