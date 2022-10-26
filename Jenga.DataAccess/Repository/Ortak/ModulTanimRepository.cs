using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repository.IRepository;
using Jenga.DataAccess.Repository.IRepository.Ortak;
using Jenga.Models.Ortak;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jenga.DataAccess.Repository.Ortak
{
    public class ModulTanimRepository : Repository<ModulTanim>, IModulTanimRepository
    {
        ApplicationDbContext _db;
        public ModulTanimRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }

        public void Update(ModulTanim obj)
        {
            _db.ModulTanim_Table.Update(obj);
        }
    }
}
