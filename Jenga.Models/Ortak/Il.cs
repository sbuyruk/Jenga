using Jenga.Models.Sistem;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Jenga.Models.Ortak
{
    public class Il : BaseModel
    {
        [Required]
        [DisplayName("İl")]
        public string? IlAdi { get; set; }
        [ValidateNever]
        [DisplayName("Plaka Kodu")]
        public int? PlakaKodu { get; set; }

        [ValidateNever]
        [DisplayName("İngilizce Adı")]
        public string? IngIlAdi { get; set; }

        [ValidateNever]
        [DisplayName("Bölge")]
        public string? Bolge { get; set; }
        [ValidateNever]
        [DisplayName("Bölge Id")]
        public int? BolgeId { get; set; }
    }
}
