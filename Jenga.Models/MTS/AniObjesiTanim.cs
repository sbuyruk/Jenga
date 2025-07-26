using Jenga.Models.Sistem;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.MTS
{
    public class AniObjesiTanim : BaseModel
    {

        [Required]
        [DisplayName("Anı Objesinin Adı")]
        public string Adi { get; set; }
        public string? StokluMu { get; set; }
        public int Sira { get; set; }
        [Required]
        public int KaynakId { get; set; }
        [ForeignKey("KaynakId")]
        [ValidateNever]
        public KaynakTanim KaynakTanim { get; set; }


    }
}
