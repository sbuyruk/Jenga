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
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repository.DYS
{
    public class MalzemeDagilimRepository : Repository<MalzemeDagilim>, IMalzemeDagilimRepository
    {
        ApplicationDbContext _db;
        public MalzemeDagilimRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }
        public IEnumerable<MalzemeDagilim> GetMalzemeDagilimWithDetails()
        {
            return _db.MalzemeDagilim_Table.Include(my => my.MalzemeYeriTanim)
                .Include(md => md.Malzeme)   // Include Malzeme
                    .ThenInclude(m => m.MalzemeCinsi)  // Include MalzemeCinsi
                        .ThenInclude(mc => mc.MalzemeGrubu)  // Include MalzemeGrup
                
                .ToList();
        }


    }
}
