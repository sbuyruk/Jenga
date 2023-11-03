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
    public class FaaliyetYeri :BaseModel
    {
        [DisplayName("Faaliyet Yeri")]
        [Required(ErrorMessage = "Faaliyet Yeri boş olamaz.")]    
        public string? Adi { get; set; }
        
        [DisplayName("Sıra")]
        [Required(ErrorMessage = "Sıra giriniz.")]
        public int? Sira { get; set; }
        
    }
}
