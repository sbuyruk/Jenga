using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Jenga.Models.Ortak
{
    public class MenuTanimVM
    {
        public MenuTanim MenuTanim { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> UstMenuSelectList { get; set; }
        [ValidateNever]
        public List<MenuTanimVM> SubMenu { get; set; }
        [ValidateNever]
        public MenuTanim UstMenuTanim { get; set; }
    }
}
