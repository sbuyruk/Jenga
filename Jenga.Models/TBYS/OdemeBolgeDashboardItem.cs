namespace Jenga.Models.TBYS
{
    /// <summary>
    /// BolgeDashboard projeksiyonu için hafif ödeme veri taşıma nesnesi.
    /// </summary>
    public sealed class OdemeBolgeDashboardItem
    {
        public DateTime? OdemeTarihi { get; set; }
        public decimal? OdenenTutar { get; set; }
        public int? SozlesmeId { get; set; }
    }
}
