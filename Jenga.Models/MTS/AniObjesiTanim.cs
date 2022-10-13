using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Jenga.Models.MTS;
using System.ComponentModel;

namespace Jenga.Models.MTS
{
    public class AniObjesiTanim
    {
        [Key]
        public int Id { get; set; }
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
        public string Aciklama { get; set; }
        public string? Olusturan { get; set; }
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        public string? Degistiren { get; set; }
        [ValidateNever]
        public DateTime DegistirmeTarihi { get; set; } = DateTime.Now;
        
    }
}
