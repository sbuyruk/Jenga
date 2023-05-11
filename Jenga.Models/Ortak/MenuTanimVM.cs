using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Jenga.Models.Ortak
{
    public class MenuTanimVM
    {
        public MenuTanim MenuTanim { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> UstMenuTanimList { get; set; }
    }
}
