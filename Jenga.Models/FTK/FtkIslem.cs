using Jenga.Models.Sistem;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.FTK
{
    [Table("FTKIslem_Table")]
    public class FtkIslem : BaseModel
    {
        [Column("Ili")]
        [DisplayName("İl")]
        public int? Ili { get; set; }

        [Column("Ilcesi")]
        [DisplayName("İlçe")]
        public int? Ilcesi { get; set; }

        [Column("ValiId")]
        [DisplayName("Vali Id")]
        public int? ValiId { get; set; }

        [Column("KaymakamId")]
        [DisplayName("Kaymakam Id")]
        public int? KaymakamId { get; set; }

        [Column("KurulusTarihi", TypeName = "date")]
        [DisplayName("Kuruluş Tarihi")]
        public DateTime? KurulusTarihi { get; set; }

        [Column("GuncellemeTarihi", TypeName = "date")]
        [DisplayName("Güncelleme Tarihi")]
        public DateTime? GuncellemeTarihi { get; set; }

        [Column("BolgeId")]
        [DisplayName("Bölge Id")]
        public int? BolgeId { get; set; }
    }
}
