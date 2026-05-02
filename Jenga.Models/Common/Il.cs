using Jenga.Models.Sistem;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Jenga.Models.Common
{
    public class Il : BaseModel
    {
        [Required]
        [DisplayName("Il")]
        public string? IlAdi { get; set; }
        [DisplayName("Plaka Kodu")]
        public int? PlakaKodu { get; set; }
        [DisplayName("Ingilizce Adi")]
        public string? IngIlAdi { get; set; }
        [DisplayName("Bölge")]
        public string? Bolge { get; set; }
        [DisplayName("Bölge Id")]
        public int? BolgeId { get; set; }
        [DisplayName("Aktif")]
        public bool? Aktif { get; set; }
    }
}
