using Jenga.Models.Attributes;
using Jenga.Models.Sistem;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Jenga.Models.TBYS
{
    public class TasinmazBagisci : BaseModel
    {
        [Required]
        [DisplayName("Adı")]
        public string? Adi { get; set; }
        [ValidateNever]
        [DisplayName("Soyadı")]
        public string? Soyadi { get; set; }
        [ValidateNever]
        [DisplayName("TC Kimlik No")]
        public long? TCKimlikNo { get; set; }
        [ValidateNever]
        [DisplayName("Doğum Yeri")]
        public string? DogumYeri { get; set; }
        [ValidateNever]
        [DisplayName("Doğum Tarihi")]
        public DateTime? DogumTarihi { get; set; }
        [ValidateNever]
        [DisplayName("İli")]
        public string? Ili { get; set; }
        [ValidateNever]
        [DisplayName("İlçesi")]
        public string? Ilcesi { get; set; }
        [ValidateNever]
        [DisplayName("İl Id")]
        public int? IlId { get; set; }
        [ValidateNever]
        [DisplayName("İlçe Id")]
        public int? IlceId { get; set; }
        [ValidateNever]
        [DisplayName("Adres")]
        public string? Adres { get; set; }
        [ValidateNever]
        [DisplayName("Telefon 1")]
        [TelefonValidation]
        public string? Telefon1 { get; set; }
        [ValidateNever]
        [DisplayName("Telefon 2")]
        [TelefonValidation]
        public string? Telefon2 { get; set; }
        [ValidateNever]
        [DisplayName("Mesleği")]
        public string? Meslegi { get; set; }
        [ValidateNever]
        [DisplayName("Sosyal Güvence")]
        public string? SosyalGuvence { get; set; }
        [ValidateNever]
        [DisplayName("Foto")]
        public string? Foto { get; set; }
        [ValidateNever]
        [DisplayName("Sağ/Vefat")]
        public string? Sag_vefat { get; set; }
        [ValidateNever]
        [DisplayName("Vefat Tarihi")]
        public DateTime? VefatTarihi { get; set; }
        [ValidateNever]
        [DisplayName("Defin Yeri")]
        public string? DefinYeri { get; set; }
        [ValidateNever]
        [DisplayName("Defin İli")]
        public string? DefinIli { get; set; }
        [ValidateNever]
        [DisplayName("Defin İlçesi")]
        public string? DefinIlcesi { get; set; }
        [ValidateNever]
        [DisplayName("Defin Açıklama")]
        public string? DefinAciklama { get; set; }
        [ValidateNever]
        [DisplayName("Gizli")]
        public bool? Gizli { get; set; }
        [ValidateNever]
        [DisplayName("E-Posta")]
        public string? EPosta { get; set; }

    }
}
