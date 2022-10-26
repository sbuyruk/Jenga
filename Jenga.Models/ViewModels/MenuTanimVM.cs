using Jenga.Models.MTS;
using Jenga.Models.Ortak;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jenga.Models.ViewModels
{
    public class MenuTanimVM
    {
        public MenuTanim MenuTanim { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> UstMenuTanimList { get; set; }
    }
}
