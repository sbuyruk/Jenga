using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Jenga.Models.MTS
{
    public class MTSGorevTanimVM
    {
        public MTSGorevTanim MTSGorevTanim { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> MTSKurumTanimList { get; set; }
    }
}
