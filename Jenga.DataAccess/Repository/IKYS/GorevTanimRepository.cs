using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repository.IRepository.IKYS;
using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Repository.IKYS
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

        public void Update(GorevTanim obj)
        {
            _db.GorevTanim_Table.Update(obj);
        }
    }
}
