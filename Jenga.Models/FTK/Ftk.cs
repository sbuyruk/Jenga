using Jenga.Models.Sistem;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.FTK
{
    [Table("FTK_Table")]
    public class Ftk : BaseModel
    {
        [Column("FTKIslemId")]
        [DisplayName("FTK İşlem Id")]
        public int? FtkIslemId { get; set; }

        [Column("Ili")]
        [DisplayName("İl")]
        public int? Ili { get; set; }

        [Column("Ilcesi")]
        [DisplayName("İlçe")]
        public int? Ilcesi { get; set; }

        [Column("KurulusTarihi", TypeName = "date")]
        [DisplayName("Kuruluş Tarihi")]
        public DateTime? KurulusTarihi { get; set; }

        [Column("GuncellemeTarihi", TypeName = "date")]
        [DisplayName("Güncelleme Tarihi")]
        public DateTime? GuncellemeTarihi { get; set; }

        [Column("FTKGorevi")]
        [DisplayName("FTK Görevi")]
        public string? FtkGorevi { get; set; }

        [Column("Adi")]
        [DisplayName("Adı")]
        public string? Adi { get; set; }

        [Column("Soyadi")]
        [DisplayName("Soyadı")]
        public string? Soyadi { get; set; }

        [Column("Unvani")]
        [DisplayName("Ünvanı")]
        public string? Unvani { get; set; }

        [Column("Telefon")]
        [DisplayName("Telefon")]
        public string? Telefon { get; set; }

        [Column("KartNo")]
        [DisplayName("Kart No")]
        public string? KartNo { get; set; }

        [Column("Sayac")]
        [DisplayName("Sayaç")]
        public int? Sayac { get; set; }

        [Column("KisiId")]
        [DisplayName("Kişi Id")]
        public int? KisiId { get; set; }

        [Column("BolgeId")]
        [DisplayName("Bölge Id")]
        public int? BolgeId { get; set; }
    }
}
