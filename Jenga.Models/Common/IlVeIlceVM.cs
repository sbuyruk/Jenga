namespace Jenga.Models.Common
{
    public class IlVeIlceVM
    {
        public int SelectedIlId { get; set; }
        public int? SelectedIlceId { get; set; }
        public IEnumerable<ListObj>? Iller { get; set; }
        public IEnumerable<ListObj>? Ilceler { get; set; }
        public string IlLabel { get; set; } = "İl";
        public string IlceLabel { get; set; } = "İlçe";
    }
}
