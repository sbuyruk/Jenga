using Jenga.DataAccess.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jenga.Models.MTS;
using Jenga.DataAccess.Repository.IRepository.MTS;
using Microsoft.EntityFrameworkCore;
using Jenga.Utility;

namespace Jenga.DataAccess.Repository.MTS
{
    public class FaaliyetAmaciRepository : Repository<FaaliyetAmaci>, IFaaliyetAmaciRepository
    {
        ApplicationDbContext _db;
        public FaaliyetAmaciRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }

    }
}
