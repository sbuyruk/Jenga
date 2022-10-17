using Jenga.Models.MTS;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jenga.Models.ViewModels
{
    public class AniObjesiVM
    {
        public AniObjesiTanim AniObjesiTanim { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> KaynakTanimList { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> StokDurumuList { get; set; }
    }
}
