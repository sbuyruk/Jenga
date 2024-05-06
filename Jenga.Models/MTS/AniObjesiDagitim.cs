using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using Jenga.Models.Sistem;

namespace Jenga.Models.MTS
{
    public class AniObjesiDagitim :BaseModel
    {

        public int AniObjesiId { get; set; }
        [DisplayName("Anı Objesi")]
        [ForeignKey("AniObjesiId")]
        [ValidateNever]
        public AniObjesiTanim AniObjesiTanim { get; set; }

        [DisplayName("Adet")]
        [ValidateNever]
        public int Adet { get; set; }

        public int? FaaliyetId { get; set; }
        [DisplayName("Faaliyet Id")]
        [ForeignKey("FaaliyetId")]
        [ValidateNever]
        public Faaliyet? Faaliyet { get; set; }
        public int KatilimciTipi { get; set; }
        public int KatilimciId { get; set; }
        public Katilimci? Katilimci { get; set; }

        [DisplayName("Veriliş Tarihi")]
        [ValidateNever]
        public DateTime? VerilisTarihi { get; set; }
        public int? CikisDepoId { get; set; }
        [DisplayName("Çıkış Deposu")]
        [ForeignKey("CikisDepoId")]
        [ValidateNever]
        public DepoTanim? DepoTanim { get; set; }
        public int? DagitimYeriTanimId { get; set; }
        [DisplayName("Dağıtım Yeri")]
        [ForeignKey("DagitimYeriTanimId")]
        [ValidateNever]
        public DagitimYeriTanim? DagitimYeriTanim { get; set; }
        public bool VerilenAlinan { get; set; }
        public string? GetirilenAniObjesi { get; set; }
    }
}
