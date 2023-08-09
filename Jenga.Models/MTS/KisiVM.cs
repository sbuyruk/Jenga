using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace Jenga.Models.MTS
{
    public class KisiVM
    {
        public Kisi Kisi { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem>? IlList { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem>? IlceList { get; set; }
    }
}
