using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Jenga.Models.Sistem;

namespace Jenga.Models.Ortak
{
    public class Bolge : BaseModel
    {
        [Required]
        [DisplayName("Bölge Adı")]
        public string Adi { get; set; }
        [Required]
        [DisplayName("Bölge Kısa Adı")]
        public string KisaAdi { get; set; }

        

    }
}
