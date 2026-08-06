namespace Jenga.Models.FTK
{
    /// <summary>
    /// BolgeDashboard projeksiyonu için hafif FTK üye veri taşıma nesnesi.
    /// </summary>
    public sealed class FtkBolgeDashboardItem
    {
        public int? FtkIslemId { get; set; }
        public int? Ili { get; set; }
        public int? Ilcesi { get; set; }
        public string? Adi { get; set; }
        public string? Soyadi { get; set; }
        public string? FtkGorevi { get; set; }
        public int? BolgeId { get; set; }
    }
}
