using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;



namespace Jenga.Models.DYS
{
    public class MalzemeDagilimVM
    {
        public MalzemeDagilim MalzemeDagilim { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> MalzemeList { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> MalzemeYeriList { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> IslemList { get; set; }
        public string Islem { get; set; }

    }
}
