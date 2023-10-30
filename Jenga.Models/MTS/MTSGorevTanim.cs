using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Jenga.Models.Sistem;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.MTS
{
    public class MTSGorevTanim : BaseModel
    {
        public int MTSKurumTanimId { get; set; }

        [DisplayName("Kurum")]
        [Required(ErrorMessage = "Kurum Adı boş olamaz.")]
        [ForeignKey("MTSKurumTanimId")]
        [ValidateNever]
        public MTSKurumTanim MTSKurumTanim { get; set; }

        [DisplayName("Görev Adı")]
        [Required(ErrorMessage = "Görev Adı boş olamaz.")]
        public string? Adi { get; set; }

        
        [DisplayName("Görev Kısa Adı")]
        [Required(ErrorMessage = "Görev Kısa Adı boş olamaz.")]
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
