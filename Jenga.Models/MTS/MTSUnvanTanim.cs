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

namespace Jenga.Models.MTS
{
    public class MTSUnvanTanim :BaseModel
    {
        [DisplayName("Ünvan")]
        [Required(ErrorMessage = "Ünvan boş olamaz.")]    
        public string? Adi { get; set; }
        
        [DisplayName("Ünvan Kısaltma")]
        [Required(ErrorMessage = "Ünvan KIsaltması boş olamaz.")]
        public string? KisaAdi { get; set; }
       
    }
}
