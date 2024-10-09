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
    public class MalzemeGrubuRepository : Repository<MalzemeGrubu>, IMalzemeGrubuRepository
    {
        ApplicationDbContext _db;
        public MalzemeGrubuRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }

        public void Update(MalzemeGrubu obj)
        {
            _db.MalzemeGrubu_Table.Update(obj);
        }
    }
}
