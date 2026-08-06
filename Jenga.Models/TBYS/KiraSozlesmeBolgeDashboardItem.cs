namespace Jenga.Models.TBYS
{
    /// <summary>
    /// BolgeDashboard projeksiyonu için hafif kira sözleşmesi veri taşıma nesnesi.
    /// </summary>
    public sealed class KiraSozlesmeBolgeDashboardItem
    {
        public int Id { get; set; }
        public DateTime? SozBasTar { get; set; }
        public DateTime? SozBitTar { get; set; }
    }
}
