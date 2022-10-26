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

namespace Jenga.DataAccess.Repository.Ortak
{
    public class MenuTanimRepository : Repository<MenuTanim>, IMenuTanimRepository
    {
        ApplicationDbContext _db;
        public MenuTanimRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }

        public void Update(MenuTanim obj)
        {
            var objFromDb = _db.MenuTanim_Table.FirstOrDefault(u => u.Id == obj.Id);
            if (objFromDb != null)
            {
                objFromDb.UstMenuId = obj.UstMenuId;
                objFromDb.Adi = obj.Adi;
                objFromDb.Aciklama = obj.Aciklama;
                objFromDb.Degistiren = obj.Degistiren;
                objFromDb.DegistirmeTarihi = obj.DegistirmeTarihi;

            }
        }

    }
}
