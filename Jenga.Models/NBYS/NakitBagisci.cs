using Jenga.Models.Sistem;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Jenga.Models.NBYS
{
    public class NakitBagisci : BaseModel
    {
        [Required]
        [DisplayName("Adı")]
        public string? Adi { get; set; }
        [Required]
        [DisplayName("Soyadı")]
        public string? Soyadi { get; set; }
        [ValidateNever]
        [DisplayName("TC Kimlik No")]
        public long? TCKimlikNo { get; set; }
        [ValidateNever]
        [DisplayName("İli")]
        public int? Ili { get; set; }
        [ValidateNever]
        [DisplayName("İlçesi")]
        public int? Ilcesi { get; set; }
        [ValidateNever]
        [DisplayName("Adres")]
        public string? Adres { get; set; }
        [ValidateNever]
        [DisplayName("Telefon 1")]
        public string? Telefon1 { get; set; }
        [ValidateNever]
        [DisplayName("Telefon 2")]
        public string? Telefon2 { get; set; }
        [ValidateNever]
        [DisplayName("Tüzel Kişi")]
        public bool TuzelKisi { get; set; }
        [ValidateNever]
        [DisplayName("Sağ/Vefat")]
        public bool Sag { get; set; }
        [ValidateNever]
        [DisplayName("E-Posta")]
        public string? Eposta { get; set; }
        [ValidateNever]
        [DisplayName("Posta Kodu")]
        public string? PostaKodu { get; set; }
        [ValidateNever]
        [DisplayName("Ulaşılamıyor")]
        public bool Ulasilamiyor { get; set; }
        [ValidateNever]
        [DisplayName("Belge İstemiyor")]
        public bool BelgeIstemiyor { get; set; }
        [ValidateNever]
        [DisplayName("Dergi Gönderilmesin")]
        public bool DergiGonderilmesin { get; set; }

    }
}
