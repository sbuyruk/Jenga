using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace Jenga.Models.MTS
{
    public class GonderiPaketiVM
    {
        public GonderiPaketi GonderiPaketi { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem>? DagitimYeriList { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem>? GondermeAraciList { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem>? IlList { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem>? IlceList { get; set; }
    }
}
