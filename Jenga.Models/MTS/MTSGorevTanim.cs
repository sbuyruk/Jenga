using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Jenga.Models.Sistem;

namespace Jenga.Models.MTS
{
    public class MTSGorevTanim : BaseModel
    {

        [DisplayName("Görev Adı")]
        [Required(ErrorMessage = "Görev Adı boş olamaz.")]
        public string? Adi { get; set; }

        [DisplayName("Görev Kısa Adı")]
        [Required(ErrorMessage = "Görev Kısa Adı boş olamaz.")]
        public string? KisaAdi { get; set; }

    }
}
