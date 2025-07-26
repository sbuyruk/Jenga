using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.DYS;
using Jenga.Models.DYS;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.DYS
{
    public class MalzemeHareketRepository : Repository<MalzemeHareket>, IMalzemeHareketRepository
    {
        ApplicationDbContext _db;
        public MalzemeHareketRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }

        public IEnumerable<MalzemeHareket> GetMalzemeHareketWithDetails()
        {
            var hareketler = _db.MalzemeHareket_Table
                .Include(m => m.Malzeme)   // Include Malzeme
                    .ThenInclude(m => m.MalzemeCinsi)  // Include MalzemeCinsi
                        .ThenInclude(mc => mc.MalzemeGrubu)  // Include MalzemeGrup
                .Include(m => m.KaynakYeri)  // Include the navigation property Kaynak
                .Include(m => m.HedefYeri)   // Include the navigation property Hedef
                .ToList();
            return hareketler;

        }
    }
}
