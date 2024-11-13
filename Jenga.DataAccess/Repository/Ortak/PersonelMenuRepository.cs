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
    public class PersonelMenuRepository : Repository<PersonelMenu>, IPersonelMenuRepository
    {
        ApplicationDbContext _db;
        public PersonelMenuRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }

        public List<PersonelMenu> GetPersonelMenuByPersonelId(int? personelId)
        {
            return _db.Set<PersonelMenu>()
                .Where(pt => pt.PersonelId == personelId)
                .ToList();
        }
    }
}
