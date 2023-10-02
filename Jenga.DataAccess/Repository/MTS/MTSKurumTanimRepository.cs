using Jenga.DataAccess.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jenga.Models.MTS;
using Jenga.DataAccess.Repository.IRepository.MTS;

namespace Jenga.DataAccess.Repository.MTS
{
    public class MTSKurumTanimRepository : Repository<MTSKurumTanim>, IMTSKurumTanimRepository
    {
        ApplicationDbContext _db;
        public MTSKurumTanimRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }

        public void Update(MTSKurumTanim obj)
        {
            _db.MTSKurumTanim_Table.Update(obj);
        }

    }
}
