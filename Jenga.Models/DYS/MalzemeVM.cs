using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace Jenga.Models.DYS
{
    public class MalzemeVM
    {
        public Malzeme Malzeme { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> MalzemeCinsiList { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> MarkaTanimList { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> ModelTanimList { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> MalzemeOzellikList { get; set; }


    }
}
