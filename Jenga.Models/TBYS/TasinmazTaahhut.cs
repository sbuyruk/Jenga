using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.TBYS
{
    [Table("TasinmazTaahhut_Table")]
    public class TasinmazTaahhut
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("TasinmazId")]
        public int? TasinmazId { get; set; }

        [Column("BagisciId")]
        public int? BagisciId { get; set; }

        [Column("Adi")]
        public string? Adi { get; set; }

        [Column("Soyadi")]
        public string? Soyadi { get; set; }

        [Column("TCKimlikNo")]
        public long? TCKimlikNo { get; set; }

        [Column("DogumTarihi")]
        public DateOnly? DogumTarihi { get; set; }

        [Column("Telefon")]
        public string? Telefon { get; set; }

        [Column("Adres")]
        public string? Adres { get; set; }

        [Column("Ili")]
        public int? Ili { get; set; }

        [Column("Ilcesi")]
        public int? Ilcesi { get; set; }

        [Column("TaahhutAciklama")]
        public string? TaahhutAciklama { get; set; }

        [Column("EvrakTarihi")]
        public DateOnly? EvrakTarihi { get; set; }

        [Column("EvrakSayisi")]
        public string? EvrakSayisi { get; set; }

        [Column("TaahhutnamePdfAdi")]
        public string? TaahhutnamePdfAdi { get; set; }

        [Column("Olusturan")]
        public string? Olusturan { get; set; }

        [Column("OlusturmaTarihi")]
        public DateTime? OlusturmaTarihi { get; set; }

        [Column("Degistiren")]
        public string? Degistiren { get; set; }

        [Column("DegistirmeTarihi")]
        public DateOnly? DegistirmeTarihi { get; set; }

        [Column("Sag_vefat")]
        public string? Sag_vefat { get; set; }

        [Column("VefatTarihi")]
        public DateOnly? VefatTarihi { get; set; }
    }
}
