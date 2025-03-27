using Jenga.Models.MTS;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;



namespace Jenga.Models.DYS
{
    public class MalzemeCinsiVM
    {
        public MalzemeCinsi MalzemeCinsi { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> MalzemeGrubuSelectList { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> UstMalzemeSelectList { get; set; }
        [ValidateNever]
        public List<MalzemeCinsiVM> SubMalzeme { get; set; }
        [ValidateNever]
        public MalzemeCinsi UstMalzemeCinsi { get; set; }
    }
}
