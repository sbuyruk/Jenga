using Jenga.DataAccess.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jenga.Models.MTS;
using Jenga.DataAccess.Repository.IRepository.MTS;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repository.MTS
{
    public class FaaliyetKatilimRepository : Repository<FaaliyetKatilim>, IFaaliyetKatilimRepository
    {
        ApplicationDbContext _db;
        public FaaliyetKatilimRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }

        public IEnumerable<FaaliyetKatilim> IncludeIt()
        {
            IEnumerable<FaaliyetKatilim> list = _db.FaaliyetKatilim_Table.Include(m => m.Katilimci);
            return list.ToList();

        }

    }
}
