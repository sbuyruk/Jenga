using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repository.IRepository.DYS;
using Jenga.DataAccess.Repository.IRepository.MTS;
using Jenga.Models.IKYS;
using Jenga.Models.DYS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Jenga.DataAccess.Repository.IRepository;
using static System.Net.Mime.MediaTypeNames;

namespace Jenga.DataAccess.Repository.DYS
{
    public class MalzemeYeriTanimRepository : Repository<MalzemeYeriTanim>, IMalzemeYeriTanimRepository
    {
        ApplicationDbContext _db;
        public MalzemeYeriTanimRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }

        public async Task<IEnumerable<SelectListItem>> GetMalzemeYeriDDL(bool onlyExistingMalzeme = false, int malzemeId = 0)
        {
            var malzemeYeriList = _db.MalzemeYeriTanim_Table.Select(m => new
            {
                MalzemeYeriTanimId = m.Id,
                Adi= m.Adi,
                Adet = _db.MalzemeDagilim_Table
                    .Where(md => md.MalzemeYeriTanimId == m.Id)
                    .Sum(md => md.Adet) 
            }).ToList();
            if (malzemeId > 0)
            {
                malzemeYeriList = _db.MalzemeYeriTanim_Table.Select(m => new
                {
                    MalzemeYeriTanimId = m.Id,
                    Adi= m.Adi,
                    Adet =  _db.MalzemeDagilim_Table
                    .Where(md => md.MalzemeId == malzemeId && md.MalzemeYeriTanimId == m.Id)
                    .Sum(md => md.Adet) 
                }).ToList();
            }
            if (onlyExistingMalzeme)
            {
                var malzemeYeriDropdownList = malzemeYeriList
                    .Where(my => my.Adet > 0)
                    .Select(m => new SelectListItem
                    {
                        Value = m.MalzemeYeriTanimId.ToString(),
                        Text = m.Adi + (m.Adet > 0 ? " (" + m.Adet + ") ": string.Empty)
                }).ToList();
                return malzemeYeriDropdownList;
            }
            else
            {
                var malzemeYeriDropdownList = malzemeYeriList.Select(m => new SelectListItem
                {
                    Value = m.MalzemeYeriTanimId.ToString(),
                    Text =  m.Adi + (m.Adet > 0 ? " (" + m.Adet + ") " : string.Empty)
                }).ToList();
                return malzemeYeriDropdownList;
            }
        }

    }
}
