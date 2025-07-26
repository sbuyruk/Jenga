using Jenga.Models.Sistem;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.MTS
{
    public class AramaGorusme : BaseModel
    {
        [Required(ErrorMessage = "Lütfen Görüşülen Kişi seçiniz.")]
        [Range(1, int.MaxValue, ErrorMessage = "Lütfen Görüşülen Kişi seçiniz.")]
        [DisplayName("Arayan Id")]
        public int ArayanId { get; set; }

        [ForeignKey("ArayanId")]
        [ValidateNever]
        public Kisi? Kisi { get; set; }
        [ValidateNever]
        [DisplayName("Faaliyet Id")]
        public int FaaliyetId { get; set; }

        [ForeignKey("FaaliyetId")]
        [ValidateNever]
        public Faaliyet? Faaliyet { get; set; }

        [Required(ErrorMessage = "Lütfen Tarih giriniz.")]
        //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
        [DisplayName("Tarih")]
        public DateTime Tarih { get; set; } = DateTime.Now;
        [ValidateNever]
        [DisplayName("Görüşme Şekli")]
        public string? GorusmeSekli { get; set; }
        [Required(ErrorMessage = "Lütfen Konu giriniz.")]
        [DisplayName("Konu")]
        public string? Konu { get; set; }
        [ValidateNever]
        [DisplayName("Görüşme Sağlandı")]
        public bool GorusmeSaglandi { get; set; } = false;
        [ValidateNever]
        [DisplayName("Randevu istendi")]
        public bool RandevuIstendi { get; set; } = false;
    }
}
