using Jenga.Models.Sistem;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Jenga.Models.Ortak
{
    public class Bolge : BaseModel
    {
        [Required]
        [DisplayName("Bölge Adı")]
        public string Adi { get; set; }
        [Required]
        [DisplayName("Bölge Kısa Adı")]
        public string KisaAdi { get; set; }



    }
}
