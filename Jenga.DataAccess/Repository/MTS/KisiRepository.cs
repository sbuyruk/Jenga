using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repository.IRepository.MTS;
using Jenga.Models.IKYS;
using Jenga.Models.MTS;
using Microsoft.EntityFrameworkCore;


namespace Jenga.DataAccess.Repository.MTS
{
    public class KisiRepository : Repository<Kisi>, IKisiRepository
    {
        ApplicationDbContext _db;
        public KisiRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }

        public void Update(Kisi obj)
        {
            _db.Kisi_Table.Update(obj);
        }
        public IEnumerable<Kisi> IncludeIt()
        {
            //IEnumerable<Kisi> list = _db.Kisi_Table.Include(m => m.MTSKurumGorevs);
            //return list;

            //IEnumerable<Kisi> list = _db.Kisi_Table.Where(a=> a.MTSKurumGorevs !=null).Include(m => m.MTSKurumGorevs.Where(n=>n.Durum.Equals("Görevde")));
            IEnumerable<Kisi> list = _db.Kisi_Table.Include(m => m.MTSKurumGorevs.Where(n => n.Durum.Equals("Görevde"))).ThenInclude(n => n.MTSKurumTanim)
                .Include(m => m.MTSKurumGorevs).ThenInclude(n => n.MTSGorevTanim);//.Where(n=>n.Durum.Equals("Görevde")));
            return list.ToList();

        }
        public Kisi? IncludeThis(int? kisiId)
        {
            if (kisiId == null)
            {
                return null;
            }
            IEnumerable<Kisi> list  = _db.Kisi_Table.Include(m => m.MTSKurumGorevs).ThenInclude(n => n.MTSKurumTanim)
                .Include(m => m.MTSKurumGorevs).ThenInclude(n => n.MTSGorevTanim).Where(a=> a.Id == kisiId).ToList();
            Kisi kisi = list.FirstOrDefault();
            return kisi;

        }
    }
}
