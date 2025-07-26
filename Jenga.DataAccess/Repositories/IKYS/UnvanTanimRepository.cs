using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.IKYS;
using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Repositories.IKYS
{
    public class UnvanTanimRepository : Repository<UnvanTanim>, IUnvanTanimRepository
    {
        ApplicationDbContext _db;
        public UnvanTanimRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }

    }
}
