using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace Jenga.Models.MTS
{
    public class MTSKurumGorevVM
    {
        
        public MTSKurumGorev MTSKurumGorev { get; set; }
        [ValidateNever]
        public Kisi Kisi { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> DurumList { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> MTSKurumTanimList { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> MTSGorevTanimList { get; set; }

    }
}
