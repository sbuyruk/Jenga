using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jenga.Models.Sistem;

namespace Jenga.Models.MTS
{
    public class DepoHareket : BaseModel
    {
        public int AniObjesiId { get; set; }
        [DisplayName("Anı Objesi")]
        [ForeignKey("AniObjesiId")]
        [ValidateNever]
        public AniObjesiTanim AniObjesiTanim { get; set; }

        public int KaynakId { get; set; }
        [DisplayName("Geldiği Kaynak")]
        [ForeignKey("KaynakId")]
        [ValidateNever]
        public DepoTanim KaynakTanim { get; set; }

        public int KaynakDepoId { get; set; }
        [DisplayName("Çıkış Yapılan Depo")]
        [ForeignKey("KaynakDepoId")]
        [ValidateNever]
        public DepoTanim KaynakDepoTanim { get; set; }
        
        public int HedefDepoId { get; set; }
        [DisplayName("Giriş Yapılan Depo")]
        [ForeignKey("HedefDepoId")]
        [ValidateNever]
        public DepoTanim DepoTanim { get; set; }
     
        [Required]
        public int Adet { get; set; }
        [Required]
        public string GirisCikis { get; set; } = "Giriş";
        [DataType(DataType.DateTime)]
        //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy hh:mm}")]
        public DateTime IslemTarihi { get; set; } = DateTime.Now;
        public string? IslemYapan { get; set; }

    }
}
