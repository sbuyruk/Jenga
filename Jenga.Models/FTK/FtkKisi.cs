using Jenga.Models.Sistem;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.FTK
{
    [Table("FTKKisi_Table")]
    public class FtkKisi : BaseModel
    {
        [Column("Adi")]
        [DisplayName("Adı")]
        public string? Adi { get; set; }

        [Column("Soyadi")]
        [DisplayName("Soyadı")]
        public string? Soyadi { get; set; }

        [Column("TCKimlikNo")]
        [DisplayName("TC Kimlik No")]
        public long? TCKimlikNo { get; set; }

        [Column("DogumTarihi", TypeName = "date")]
        [DisplayName("Doğum Tarihi")]
        public DateTime? DogumTarihi { get; set; }

        [Column("Telefon1")]
        [DisplayName("Telefon 1")]
        public string? Telefon1 { get; set; }

        [Column("Telefon2")]
        [DisplayName("Telefon 2")]
        public string? Telefon2 { get; set; }

        [Column("Unvani")]
        [DisplayName("Ünvanı")]
        public string? Unvani { get; set; }

        [Column("Vali")]
        [DisplayName("Vali")]
        public bool? Vali { get; set; }

        [Column("Kaymakam")]
        [DisplayName("Kaymakam")]
        public bool? Kaymakam { get; set; }

        [Column("Ili")]
        [DisplayName("İl")]
        public int? Ili { get; set; }

        [Column("Ilcesi")]
        [DisplayName("İlçe")]
        public int? Ilcesi { get; set; }

        [Column("Adres")]
        [DisplayName("Adres")]
        public string? Adres { get; set; }

        [Column("FTKGorevi")]
        [DisplayName("FTK Görevi")]
        public int? FtkGorevi { get; set; }

        [Column("UyelikDurumu")]
        [DisplayName("Üyelik Durumu")]
        public string? UyelikDurumu { get; set; }

        [Column("KartNo")]
        [DisplayName("Kart No")]
        public string? KartNo { get; set; }
    }
}
