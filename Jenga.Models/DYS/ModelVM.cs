using Jenga.Models.MTS;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace Jenga.Models.DYS
{
    public class ModelVM
    {
        public ModelTanim ModelTanim { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> MarkaTanimList { get; set; }

    }
}
