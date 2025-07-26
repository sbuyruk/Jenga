using Jenga.Models.Sistem;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Jenga.Models.MTS
{
    public class MTSKurumTanim : BaseModel
    {
        [DisplayName("Kurum Adı")]
        [Required(ErrorMessage = "Kurum Adı boş olamaz.")]
        public string? Adi { get; set; }

        [DisplayName("Kurum Kısa Adı")]
        [Required(ErrorMessage = "Kurum Kısa Adı boş olamaz.")]
        public string? KisaAdi { get; set; }
        [ValidateNever]
        [DisplayName("Telefon 1")]
        public string? Telefon1 { get; set; }
        [ValidateNever]
        [DisplayName("Telefon 2")]
        public string? Telefon2 { get; set; }
        [ValidateNever]
        [DisplayName("Telefon 3")]
        public string? Telefon3 { get; set; }
        [ValidateNever]
        [DisplayName("Telefon 4")]
        public string? Telefon4 { get; set; }
        [ValidateNever]
        [DisplayName("Telefon 5")]
        public string? Telefon5 { get; set; }
        [ValidateNever]
        [DisplayName("Açıklama 1")]
        public string? TelAciklama1 { get; set; }
        [ValidateNever]
        [DisplayName("Açıklama 2")]
        public string? TelAciklama2 { get; set; }
        [ValidateNever]
        [DisplayName("Açıklama 3")]
        public string? TelAciklama3 { get; set; }
        [ValidateNever]
        [DisplayName("Açıklama 4")]
        public string? TelAciklama4 { get; set; }
        [ValidateNever]
        [DisplayName("Açıklama 5")]
        public string? TelAciklama5 { get; set; }
    }
}
