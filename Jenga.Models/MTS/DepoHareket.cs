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
        public int DepoId { get; set; }
        [DisplayName("Depo/Yer")]
        [ForeignKey("DepoId")]
        [ValidateNever]
        public DepoTanim DepoTanim { get; set; }
        [Required]
        public int Adet { get; set; }
        [Required]
        public string GirisCikis { get; set; } = "Giriş";
        public string Aciklama { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime IslemTarihi { get; set; } = DateTime.Now;
        public string? IslemYapan { get; set; }

    }
}
