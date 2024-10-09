using Jenga.Models.MTS;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace Jenga.Models.DYS
{
    public class MalzemeOzellikVM
    {
        public MalzemeOzellik MalzemeOzellik { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> OzellikList { get; set; }

    }
}
