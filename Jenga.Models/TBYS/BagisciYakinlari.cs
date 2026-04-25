using Jenga.Models.Sistem;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.TBYS
{
    [Table("BagisciYakinlari_Table")]
    public class BagisciYakinlari : BaseModel
    {
        [Column("AdSoyad")]
        public string? AdSoyad { get; set; }

        [Column("Telefon")]
        public string? Telefon { get; set; }

        [Column("YakinlikDerecesi")]
        public string? YakinlikDerecesi { get; set; }

        [Column("Sira")]
        public int? Sira { get; set; }

        [Column("BagisciId")]
        public long? BagisciId { get; set; }
    }
}
