using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.Ortak;
using Jenga.Models.Ortak;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace Jenga.DataAccess.Repositories.Ortak
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
        public async Task<IEnumerable<SelectListItem>> GetIlDDL(int bolgeId = 0)
        {
            var ilList = _db.Il_Table
                 .Select(m => new
                 {
                     m.Id,
                     Adi = m.IlAdi,
                     Bolge = m.BolgeId
                 })
                 .ToList();
            if (bolgeId > 0)
            {
                ilList = _db.Il_Table.Where(i => i.BolgeId == bolgeId)
                 .Select(m => new
                 {
                     m.Id,
                     Adi = m.IlAdi,
                     Bolge = m.BolgeId
                 })
                 .ToList();
            }
            var ilDropdownList = ilList
                .Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = m.Adi,
                }).ToList();
            return ilDropdownList;
        }


    }
}
