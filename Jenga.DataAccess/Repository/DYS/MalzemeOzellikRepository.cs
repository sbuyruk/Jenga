using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repository.IRepository.DYS;
using Jenga.DataAccess.Repository.IRepository.MTS;
using Jenga.Models.IKYS;
using Jenga.Models.DYS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jenga.DataAccess.Repository.DYS
{
    public class MalzemeOzellikRepository : Repository<MalzemeOzellik>, IMalzemeOzellikRepository
    {
        ApplicationDbContext _db;
        public MalzemeOzellikRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }

        public void Update(MalzemeOzellik obj)
        {
            _db.MalzemeOzellik_Table.Update(obj);
        }
    }
}
