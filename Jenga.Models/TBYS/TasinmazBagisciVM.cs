using Jenga.Models.Ortak;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace Jenga.Models.TBYS
{
    public class TasinmazBagisciVM
    {
        public TasinmazBagisci TasinmazBagisci { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> SosyalGuvenceList { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> SagVefatList { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> DefinIliList { get; set; }        
        [ValidateNever]
        public IEnumerable<SelectListItem> DefinIlcesiList { get; set; }
        [ValidateNever]

        public IlVeIlceVM IlVeIlceVM { get; set; } = new IlVeIlceVM();
    }
}
