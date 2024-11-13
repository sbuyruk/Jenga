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
    public class IlRepository : Repository<Il>, IIlRepository
    {
        ApplicationDbContext _db;
        public IlRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }
        public List<Il> GetPersonelMenuByPersonelId(int? personelId)
        {
            return _db.Set<Il>()
                .Where(pt => pt.Id == personelId)
                .ToList();
        }
    }
}
