using Jenga.Models.DYS;
using Jenga.Models.IKYS;
using Jenga.Models.MTS;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jenga.DataAccess.Repository.IRepository.DYS
{
    public interface IMalzemeRepository : IRepository<Malzeme>
    {
        Task<IEnumerable<SelectListItem>> GetMalzemeDDL(bool onlyExistingMalzeme = false);
    }
}
