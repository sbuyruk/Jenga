using Jenga.Models.Sistem;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.NBYS
{
    [Table("NakitBagisci_Table")]
    public class NakitBagisci : BaseModel
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

        [Column("Ili")]
        [DisplayName("İli")]
        public int? Ili { get; set; }

        [Column("Ilcesi")]
        [DisplayName("İlçesi")]
        public int? Ilcesi { get; set; }

        [Column("Adres")]
        [DisplayName("Adres")]
        public string? Adres { get; set; }

        [Column("Telefon1")]
        [DisplayName("Telefon 1")]
        public string? Telefon1 { get; set; }

        [Column("Telefon2")]
        [DisplayName("Telefon 2")]
        public string? Telefon2 { get; set; }

        [Column("TuzelKisi")]
        [DisplayName("Tüzel Kişi")]
        public bool? TuzelKisi { get; set; }

        // BaseModel already contains: OlusturmaTarihi, Olusturan, DegistirmeTarihi, Degistiren, Aciklama

        [Column("Sag")]
        [DisplayName("Sağ")]
        public bool? Sag { get; set; }

        [Column("Eposta")]
        [DisplayName("E-posta")]
        public string? Eposta { get; set; }

        [Column("PostaKodu")]
        [DisplayName("Posta Kodu")]
        public string? PostaKodu { get; set; }

        [Column("Ulasilamiyor")]
        [DisplayName("Ulaşılamıyor")]
        public bool? Ulasilamiyor { get; set; }

        [Column("BelgeIstemiyor")]
        [DisplayName("Belge İstemiyor")]
        public bool? BelgeIstemiyor { get; set; } = false;

        [Column("VefatTarihi", TypeName = "date")]
        [DisplayName("Vefat Tarihi")]
        public DateTime? VefatTarihi { get; set; }

        [Column("DergiGonderilmesin")]
        [DisplayName("Dergi Gönderilmesin")]
        public bool? DergiGonderilmesin { get; set; }

        [ValidateNever]
        [NotMapped]
        public string AdSoyad => $"{Adi} {Soyadi}".Trim();
    }
}