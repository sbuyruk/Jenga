using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.DYS;
using Jenga.Models.DYS;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Jenga.DataAccess.Repositories.DYS
{
    public class MalzemeRepository : Repository<Malzeme>, IMalzemeRepository
    {
        ApplicationDbContext _db;
        public MalzemeRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }
        public async Task<IEnumerable<SelectListItem>> GetMalzemeDDL(bool onlyExistingMalzeme = false)
        {
            var malzemeList = _db.Malzeme_Table
                .Select(m => new
                {
                    m.Id,
                    m.Adi,
                    AdetSum = _db.MalzemeDagilim_Table.Where(md => md.MalzemeId == m.Id)
                        .Sum(a => (int?)a.Adet) ?? 0 // If no records found, default to 0
                })
                .ToList();
            if (onlyExistingMalzeme)
            {
                var malzemeDropdownList = malzemeList.Where(m => m.AdetSum > 0).Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = m.Adi + " (" + m.AdetSum + ")",
                }).ToList();
                return malzemeDropdownList;
            }
            else
            {
                var malzemeDropdownList = malzemeList.Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = m.Adi + " (" + m.AdetSum + ")",
                }).ToList();
                return malzemeDropdownList;
            }

        }
    }
}
