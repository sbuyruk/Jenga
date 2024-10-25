using Jenga.Models.MTS;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace Jenga.Models.DYS
{
    public class MalzemeHareketVM
    {
        public MalzemeHareket MalzemeHareket { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> MalzemeList { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> KaynakYeriList { get; set; } // For KaynakYeriId
        [ValidateNever]
        public IEnumerable<SelectListItem> HedefYeriList { get; set; }  // For HedefYeriId
        [ValidateNever]
        public IEnumerable<SelectListItem> GirisCikisList { get; set; }

    }
}
