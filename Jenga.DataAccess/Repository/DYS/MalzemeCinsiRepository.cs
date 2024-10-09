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
    public class MalzemeCinsiRepository : Repository<MalzemeCinsi>, IMalzemeCinsiRepository
    {
        ApplicationDbContext _db;
        public MalzemeCinsiRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }

        public void Update(MalzemeCinsi obj)
        {
            _db.MalzemeCinsi_Table.Update(obj);
        }
    }
}
