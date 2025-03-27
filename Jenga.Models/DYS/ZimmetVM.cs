using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace Jenga.Models.DYS
{
    public class ZimmetVM
    {
        public Zimmet Zimmet { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> MalzemeList { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> PersonelList { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> MalzemeYeriList { get; set; }


    }
}
