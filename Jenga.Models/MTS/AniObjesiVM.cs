using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace Jenga.Models.MTS
{
    public class AniObjesiVM
    {
        public AniObjesiTanim AniObjesiTanim { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> KaynakTanimList { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> StokDurumuList { get; set; }
    }
}
