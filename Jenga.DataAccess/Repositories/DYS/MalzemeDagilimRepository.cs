using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.DYS;
using Jenga.Models.DYS;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.DYS
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
