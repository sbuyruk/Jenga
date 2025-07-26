using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.IKYS;
using Jenga.Models.IKYS;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.IKYS
{
    public class PersonelRepository : Repository<Personel>, IPersonelRepository
    {
        ApplicationDbContext _db;
        public PersonelRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }
        public async Task<IEnumerable<SelectListItem>> GetPersonelDDL(bool onlyWorkingPersonel = true, int malzemeId = 0)
        {
            IQueryable<Personel> query = dbSet;

            //var personelList = await query.Where(u => u.IsBilgileri.CalismaDurumu == "1")
            //                .Select(y => new SelectListItem
            //                {
            //                    Value = y.Id.ToString(),
            //                    Text = y.Adi + " " + y.Soyadi
            //                }).ToListAsync();
            //return  personelList;



            var personelList = await query.Where(u => u.IsBilgileri.CalismaDurumu == "1")
               .Select(p => new
               {
                   p.Id,
                   Adi = p.Adi + " " + p.Soyadi,
                   AdetSum = _db.Zimmet_Table.Where(z => z.PersonelId == p.Id && z.MalzemeId == malzemeId)
                       .Sum(z => (int?)z.Adet) ?? 0 // If no records found, default to 0
               })
               .ToListAsync();
            var personelDropdownList = personelList.Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = m.Adi + (m.AdetSum > 0 ? " (" + m.AdetSum + ") " : string.Empty)
            }).ToList();
            return personelDropdownList;
        }
    }
}
