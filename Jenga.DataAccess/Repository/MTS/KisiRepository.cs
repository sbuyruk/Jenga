using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repository.IRepository.MTS;
using Jenga.Models.IKYS;
using Jenga.Models.MTS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
