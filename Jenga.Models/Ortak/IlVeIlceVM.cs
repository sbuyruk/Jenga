using Microsoft.AspNetCore.Mvc.Rendering;


namespace Jenga.Models.Ortak
{
    public class IlVeIlceVM
    {
        public int SelectedIlId { get; set; }
        public int? SelectedIlceId { get; set; }
        public IEnumerable<SelectListItem>? Iller { get; set; }
        public IEnumerable<SelectListItem>? Ilceler { get; set; }
        public string IlLabel { get; set; } = "İl"; // Varsayılan etiket
        public string IlceLabel { get; set; } = "İlçe"; // Varsayılan etiket
    }
}
