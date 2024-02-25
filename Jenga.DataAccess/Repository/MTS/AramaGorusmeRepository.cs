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
    public class AramaGorusmeRepository : Repository<AramaGorusme>, IAramaGorusmeRepository
    {
        ApplicationDbContext _db;
        public AramaGorusmeRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }

        public void Update(AramaGorusme obj)
        {
            _db.AramaGorusme_Table.Update(obj);
        }

    }
}
