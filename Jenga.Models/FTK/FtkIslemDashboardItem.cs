namespace Jenga.Models.FTK
{
    /// <summary>
    /// BolgeDashboard projeksiyonu için hafif FTK işlem veri taşıma nesnesi.
    /// </summary>
    public sealed class FtkIslemDashboardItem
    {
        public int Id { get; set; }
        public DateTime? KurulusTarihi { get; set; }
        public DateTime? GuncellemeTarihi { get; set; }
    }
}
