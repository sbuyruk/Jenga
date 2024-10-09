using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repository.IRepository.MTS;
using Jenga.Models.DYS;
using Jenga.Models.MTS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jenga.DataAccess.Repository.MTS
{
    public class DepoTanimRepository : Repository<DepoTanim>, IDepoTanimRepository
    {
        ApplicationDbContext _db;
        public DepoTanimRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }

        public void Update(DepoTanim obj)
        {
            _db.DepoTanim_Table.Update(obj);
        }
    }
}
