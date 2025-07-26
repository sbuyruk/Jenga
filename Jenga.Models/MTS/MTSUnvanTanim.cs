using Jenga.Models.Sistem;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Jenga.Models.MTS
{
    public class MTSUnvanTanim : BaseModel
    {
        [DisplayName("Ünvan")]
        [Required(ErrorMessage = "Ünvan boş olamaz.")]
        public string? Adi { get; set; }

        [DisplayName("Ünvan Kısaltma")]
        [Required(ErrorMessage = "Ünvan KIsaltması boş olamaz.")]
        public string? KisaAdi { get; set; }

    }
}
