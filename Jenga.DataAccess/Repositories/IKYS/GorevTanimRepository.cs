using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.IKYS;
using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Repositories.IKYS
{
    public class GorevTanimRepository : Repository<GorevTanim>, IGorevTanimRepository
    {
        ApplicationDbContext _db;
        public GorevTanimRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }

    }
}
