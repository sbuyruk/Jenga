namespace Jenga.Models.NBYS
{
    /// <summary>
    /// Dashboard projeksiyonu için hafif bağışçı veri taşıma nesnesi.
    /// </summary>
    public sealed class NakitBagisciDashboardItem
    {
        public int Id { get; set; }
        public int? Ili { get; set; }
        public bool? TuzelKisi { get; set; }
    }
}
