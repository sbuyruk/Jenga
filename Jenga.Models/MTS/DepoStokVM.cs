using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jenga.Models.MTS
{
    public class DepoStokVM
    {
        public DepoStok DepoStok { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> DepoTanimList { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> AniObjesiList { get; set; }
    }
}
