using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repository.IRepository;
using Jenga.Models.MTS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jenga.DataAccess.Repository
{
    public class KaynakTanimRepository : Repository<KaynakTanim>, IKaynakTanimRepository
    {
        ApplicationDbContext _db;
        public KaynakTanimRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }

        public void Update(KaynakTanim obj)
        {
            _db.KaynakTanim_Table.Update(obj);
        }
    }
}
