using Jenga.Models.Sistem;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.Common
{
    public class Ilce : BaseModel
    {
        [Required]
        public int IlId { get; set; }
        [DisplayName("Il")]
        [ForeignKey("IlId")]
        public Il IlTanim { get; set; }

        [Required]
        [DisplayName("Il Adi")]
        public string? IlAdi { get; set; }

        [Required]
        public int IlceId { get; set; }

        [Required]
        [DisplayName("Ilçe Adi")]
        public string? IlceAdi { get; set; }
        [Required]
        [DisplayName("Aktif")]
        public bool? Aktif { get; set; }
    }
}
