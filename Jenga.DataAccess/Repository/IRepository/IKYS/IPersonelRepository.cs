using Jenga.Models.IKYS;
using Jenga.Models.Ortak;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jenga.DataAccess.Repository.IRepository.IKYS
{
    public interface IPersonelRepository : IRepository<Personel>
    {
        Task<IEnumerable<SelectListItem>> GetPersonelDDL(bool onlyWorkingPersonel = true, int malzemeId = 0);
    }
}
