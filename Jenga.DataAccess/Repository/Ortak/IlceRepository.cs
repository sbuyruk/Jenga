using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jenga.Models.MTS;
using Jenga.Models.Ortak;
using Jenga.DataAccess.Repository.IRepository.Ortak;
using Jenga.DataAccess.Repository.IRepository.IKYS;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repository.Ortak
{
    public class IlceRepository : Repository<Ilce>, IIlceRepository
    {
        ApplicationDbContext _db;
        public IlceRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }

        public bool Update(Ilce obj)
        {
            return false;
        }
        public List<Ilce> GetPersonelMenuByPersonelId(int? ilId)
        {
            return _db.Set<Ilce>()
                .Where(pt => pt.IlId == ilId)
                .ToList();
        }
    }
}
