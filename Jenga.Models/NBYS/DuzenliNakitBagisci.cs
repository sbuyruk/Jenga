using Jenga.Models.Sistem;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.NBYS
{
    [Table("DuzenliNakitBagisci_Table")]
    public class DuzenliNakitBagisci : BaseModel
    {
        [Column("BagisciId")]
        [DisplayName("Bağışçı Id")]
        public int? BagisciId { get; set; }

        [Column("TCKimlikNo")]
        [DisplayName("TC Kimlik No")]
        public long? TCKimlikNo { get; set; }

        [Column("BagisciAdi")]
        [DisplayName("Bağışçı Adı")]
        public string? BagisciAdi { get; set; }

        [Column("BaslamaTarihi", TypeName = "date")]
        [DisplayName("Başlama Tarihi")]
        public DateTime? BaslamaTarihi { get; set; }

        [Column("BitisTarihi", TypeName = "date")]
        [DisplayName("Bitiş Tarihi")]
        public DateTime? BitisTarihi { get; set; }

        [Column("Tutar", TypeName = "money")]
        [DisplayName("Tutar")]
        public decimal? Tutar { get; set; }

        [Column("BagisAdedi")]
        [DisplayName("Bağış Adedi")]
        public int? BagisAdedi { get; set; }

        [Column("BagisToplami", TypeName = "decimal(18, 0)")]
        [DisplayName("Bağış Toplamı")]
        public decimal? BagisToplami { get; set; }

        [Column("Aktif")]
        [DisplayName("Aktif")]
        public bool? Aktif { get; set; }

        [Column("ArmaganId")]
        [DisplayName("Armağan Id")]
        public int? ArmaganId { get; set; }

        [Column("NakitBagisHareketId")]
        [DisplayName("Nakit Bağış Hareket Id")]
        public int? NakitBagisHareketId { get; set; }

        [Column("Telefon")]
        [DisplayName("Telefon")]
        public string? Telefon { get; set; }

        [Column("EPosta")]
        [DisplayName("E-Posta")]
        public string? EPosta { get; set; }

        [Column("EslesmeBilgisi")]
        [DisplayName("Eşleşme Bilgisi")]
        public string? EslesmeBilgisi { get; set; }

        // BaseModel already contains: Id, Aciklama, Olusturan, OlusturmaTarihi, Degistiren, DegistirmeTarihi
    }
}
