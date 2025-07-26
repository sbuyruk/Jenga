using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace Jenga.Models.DYS
{
    public class OzellikVM
    {
        public Ozellik Ozellik { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> MalzemeCinsiList { get; set; }

    }
}
