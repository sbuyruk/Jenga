using Jenga.Models.Sistem;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.NBYS
{
    [Table("ArmaganTanim_Table")]
    public class ArmaganTanim : BaseModel
    {
        [Column("Armagan")]
        [DisplayName("Armağan")]
        public string? Armagan { get; set; }

        [Column("OzelKisiAltLimit", TypeName = "money")]
        [DisplayName("Özel Kişi Alt Limit")]
        public decimal? OzelKisiAltLimit { get; set; }

        [Column("OzelKisiUstLimit", TypeName = "money")]
        [DisplayName("Özel Kişi Üst Limit")]
        public decimal? OzelKisiUstLimit { get; set; }

        [Column("TuzelKisiAltLimit", TypeName = "money")]
        [DisplayName("Tüzel Kişi Alt Limit")]
        public decimal? TuzelKisiAltLimit { get; set; }

        [Column("TuzelKisiUstLimit", TypeName = "money")]
        [DisplayName("Tüzel Kişi Üst Limit")]
        public decimal? TuzelKisiUstLimit { get; set; }

        [Column("ImzaGorevi")]
        [DisplayName("İmza Görevi")]
        public string? ImzaGorevi { get; set; }

        [Column("ImzaAdi")]
        [DisplayName("İmza Adı")]
        public string? ImzaAdi { get; set; }

        [Column("Sira")]
        [DisplayName("Sıra")]
        public int? Sira { get; set; }

        [Column("Aktif")]
        [DisplayName("Aktif")]
        public bool? Aktif { get; set; }

        [Column("KisaArmagan")]
        [DisplayName("Kısa Armağan")]
        public string? KisaArmagan { get; set; }

        // BaseModel already contains: Id, Aciklama, Olusturan, OlusturmaTarihi, Degistiren, DegistirmeTarihi
    }
}
