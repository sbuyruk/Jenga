using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Jenga.Models.MTS
{
    public class DepoHareketVM
    {
        public DepoHareket DepoHareket { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> KaynakList { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> KaynakDepoList { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> HedefDepoList { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> AniObjesiList { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> GirisCikisList { get; set; }
    }
}
