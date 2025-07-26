using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace Jenga.Models.MTS
{
    public class AniObjesiDagitimVM
    {
        public AniObjesiDagitim AniObjesiDagitim { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> KatilimciList { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> AniObjesiTanimList { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> DepoTanimList { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> DagitimYeriTanimList { get; set; }
        public IEnumerable<SelectListItem> VerilenAlinanList { get; set; }
    }
}
