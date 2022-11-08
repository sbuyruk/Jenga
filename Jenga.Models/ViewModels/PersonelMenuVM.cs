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
    public class PersonelMenuVM
    {
        public PersonelMenu PersonelMenu { get; set; }
        public IEnumerable<SelectListItem> PersonelList { get; set; }
        public IEnumerable<SelectListItem> MenuTanimList { get; set; } 
    }
}
