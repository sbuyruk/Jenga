using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using Jenga.Models.Sistem;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Jenga.Models.TYS;

namespace Jenga.Models.MTS
{
    public class ToplantiKatilim :BaseModel
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
