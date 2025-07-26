using Jenga.Models.Sistem;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.DYS
{
    public class MalzemeDagilim : BaseModel
    {
        public int MalzemeId { get; set; }
        [DisplayName("Malzeme")]
        [ForeignKey("MalzemeId")]
        [ValidateNever]
        public Malzeme Malzeme { get; set; }
        [DisplayName("Malzeme Yeri")]
        public int MalzemeYeriTanimId { get; set; }
        [ForeignKey("MalzemeYeriTanimId")]
        [ValidateNever]
        public MalzemeYeriTanim MalzemeYeriTanim { get; set; }
        [Required]
        public int Adet { get; set; }
        [ValidateNever]
        [DisplayName("Tarih")]
        public DateTime Tarih { get; set; } = DateTime.Now;
    }
}
