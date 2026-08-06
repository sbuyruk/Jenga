namespace Jenga.Models.NBYS
{
    /// <summary>
    /// Dashboard projeksiyonu için hafif veri taşıma nesnesi.
    /// Sadece NakitDashboardPage'in ihtiyaç duyduğu alanları içerir.
    /// </summary>
    public sealed class NakitBagisDashboardItem
    {
        public DateTime? BagisTarihi { get; set; }
        public decimal? BagisMiktari { get; set; }
        public int? BagisciId { get; set; }
        public bool? IadeEdildiMi { get; set; }
    }
}
