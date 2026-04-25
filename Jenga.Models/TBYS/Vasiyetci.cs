using Jenga.Models.Sistem;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.TBYS
{
    [Table("Vasiyetci_Table")]
    public class Vasiyetci : BaseModel
    {
        [Column("Adi")]
        public string? Adi { get; set; }

        [Column("Soyadi")]
        public string? Soyadi { get; set; }

        [Column("TCKimlikNo")]
        public long? TCKimlikNo { get; set; }

        [Column("DogumTarihi", TypeName = "date")]
        public DateTime? DogumTarihi { get; set; }

        [Column("DogumYeri")]
        public string? DogumYeri { get; set; }

        [Column("SagVefat")]
        public string? SagVefat { get; set; }

        [Column("VefatTarihi", TypeName = "date")]
        public DateTime? VefatTarihi { get; set; }

        [Column("Telefon1")]
        public string? Telefon1 { get; set; }

        [Column("Telefon2")]
        public string? Telefon2 { get; set; }

        [Column("IkametIli")]
        public int? IkametIli { get; set; }

        [Column("IkametIlcesi")]
        public int? IkametIlcesi { get; set; }

        [Column("IkametAdresi")]
        public string? IkametAdresi { get; set; }

        [Column("VasiyetTipi")]
        public string? VasiyetTipi { get; set; }

        [Column("VasiyetinDurumu")]
        public string? VasiyetinDurumu { get; set; }

        [Column("Noter")]
        public string? Noter { get; set; }

        [Column("VasiyetTarihi", TypeName = "date")]
        public DateTime? VasiyetTarihi { get; set; }

        [Column("YevmiyeNumarasi")]
        public string? YevmiyeNumarasi { get; set; }

        [Column("VasiyetcininTalebi")]
        public string? VasiyetcininTalebi { get; set; }
    }
}
