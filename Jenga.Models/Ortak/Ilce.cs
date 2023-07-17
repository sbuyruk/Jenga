using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Jenga.Models.Sistem;
using Jenga.Models.MTS;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.Ortak
{
    public class Ilce : BaseModel
    {
        [Required]
        public int IlId { get; set; }
        [DisplayName("İl")]
        [ForeignKey("IlId")]
        [ValidateNever]
        public Il IlTanim{ get; set; }

        [Required]
        [DisplayName("İl Adı")]
        public string? IlAdi { get; set; }

        [Required]
        public int IlceId { get; set; }

        [Required]
        [DisplayName("İlçe Adı")]
        public string? IlceAdi { get; set; }        
    }
}
