using Jenga.Models.IKYS;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Jenga.Models.Ortak
{
    public class GorevTanimVM
    {
        public GorevTanim GorevTanim { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> TanimList { get; set; }
    }
}
