using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jenga.Models.Ortak;

namespace Jenga.Models.MTS
{
    public class DepoStok : BaseModel
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
        public int SonAdet { get; set; }
        [DataType(DataType.DateTime)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime SonIslemTarihi { get; set; } = DateTime.Now;
        public string? SonIslemYapan { get; set; }
        [ValidateNever]
        public string Aciklama { get; set; }

    }
}
