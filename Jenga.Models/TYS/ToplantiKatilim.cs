using Jenga.Models.Sistem;
using Jenga.Models.TYS;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.MTS
{
    public class ToplantiKatilim : BaseModel
    {
        public int ToplantiId { get; set; }
        [DisplayName("Toplantı")]
        [ForeignKey("ToplantiId")]
        [ValidateNever]
        public Toplanti? Toplanti { get; set; }

        [DisplayName("Katilimci Id")]
        [Required(ErrorMessage = "Katilimci Id boş olamaz.")]
        public int KatilimciId { get; set; }
        [ForeignKey("KatilimciId")]
        [ValidateNever]
        public Kisi? Kisi { get; set; }
        [ValidateNever]
        public bool? Bilgi { get; set; }

    }
}
