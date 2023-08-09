using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repository.IRepository.MTS;
using Jenga.Models.MTS;


namespace Jenga.DataAccess.Repository.MTS
{
    public class RandevuRepository : Repository<Randevu>, IRandevuRepository
    {
        ApplicationDbContext _db;
        public RandevuRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }

        public void Update(Randevu obj)
        {
            _db.Randevu_Table.Update(obj);
        }
    }
}
