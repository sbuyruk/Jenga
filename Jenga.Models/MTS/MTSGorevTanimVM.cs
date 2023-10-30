using Jenga.Models.Ortak;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jenga.Models.MTS
{
    public class MTSGorevTanimVM
    {
        public MTSGorevTanim MTSGorevTanim { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> MTSKurumTanimList { get; set; }
    }
}
