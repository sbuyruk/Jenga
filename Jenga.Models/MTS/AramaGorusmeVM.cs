using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;


namespace Jenga.Models.MTS
{
    public class AramaGorusmeVM
    {
        public AramaGorusme AramaGorusme { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> GorusmeSekliList { get; set; }
        [ValidateNever]
        public Katilimci? GorusulenKatilimci { get; set; }
        public string KatilimciBilgisi { //custom get method
            get
            {
                return GorusulenKatilimci?.Adi + " " + GorusulenKatilimci?.Soyadi;
            } 
            set { }

        }
    }
}
