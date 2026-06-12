namespace Jenga.Models.NBYS
{
    /// <summary>
    /// ArmaganDashboard projeksiyonu için hafif veri taşıma nesnesi.
    /// Sadece dashboard'un ihtiyaç duyduğu alanları içerir.
    /// </summary>
    public sealed class ArmaganDashboardItem
    {
        public int? BagisciId { get; set; }
        public int? ArmaganTanimId { get; set; }
        public DateTime? Tarih { get; set; }
        public string? Durum { get; set; }
        public decimal? BagisMiktari { get; set; }
        public string? DovizCinsi { get; set; }
        public string? BelgedeYazanIsim { get; set; }
        public bool? DuzenliBagis { get; set; }
        public bool? CokluBagis { get; set; }
    }
}
