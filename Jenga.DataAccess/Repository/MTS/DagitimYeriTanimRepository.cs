using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repository.IRepository.MTS;
using Jenga.Models.MTS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jenga.DataAccess.Repository.MTS
{
    public class DagitimYeriTanimRepository : Repository<DagitimYeriTanim>, IDagitimYeriTanimRepository
    {
        ApplicationDbContext _db;
        public DagitimYeriTanimRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }

        public void Update(DagitimYeriTanim obj)
        {
            _db.DagitimYeriTanim_Table.Update(obj);
        }
    }
}
