using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repository.IRepository.IKYS;
using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Repository.IKYS
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

        public void Update(Kimlik obj)
        {
            _db.Kimlik_Table.Update(obj);
        }
    }
}
