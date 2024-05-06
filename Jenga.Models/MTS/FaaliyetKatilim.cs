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

namespace Jenga.Models.MTS
{
    public class FaaliyetKatilim :BaseModel
    {
        [DisplayName("Faaliyet Id")]
        [Required(ErrorMessage = "Faaliyet Id boş olamaz.")]
        public int FaaliyetId { get; set; }
        //[ForeignKey("FaaliyetId")]
        //[ValidateNever]
        //public Faaliyet Faaliyet{ get; set; }
        [DisplayName("Katilimci Id")]
        [Required(ErrorMessage = "Katilimci Id boş olamaz.")]
        public int KatilimciId { get; set; }
        
        [ForeignKey("KatilimciId")]
        [ValidateNever]
        public Kisi? Kisi { get; set; }


        [DisplayName("Katilimci Tipi")]
        [Required(ErrorMessage = "Katilimci Tipi boş olamaz.")]
        [ForeignKey("KatilimciTipi")]
        public int KatilimciTipi { get; set; }
        [ValidateNever]
        [DisplayName("Kurum/Görev")]
        public string? KurumGorev { get; set; }
        [ValidateNever]
        [DisplayName("Takvim Daveti")]
        public string? TakvimDaveti { get; set; }

        [NotMapped]
        public Katilimci? Katilimci { get; set; }

    }
}
