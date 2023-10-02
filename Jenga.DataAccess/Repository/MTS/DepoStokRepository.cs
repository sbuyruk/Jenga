using Jenga.DataAccess.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jenga.Models.MTS;
using Jenga.DataAccess.Repository.IRepository.MTS;
using Jenga.DataAccess.Repository.IRepository.NBYS;

namespace Jenga.DataAccess.Repository.MTS
{
    public class DepoStokRepository : Repository<DepoStok>, IDepoStokRepository
    {
        ApplicationDbContext _db;
        public DepoStokRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }

        public bool Update(DepoStok obj)
        {
            var updated = _db.DepoStok_Table.Update(obj);
            return updated!=null;
        }

    }
}
