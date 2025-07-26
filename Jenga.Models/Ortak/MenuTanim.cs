using Jenga.Models.Sistem;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Jenga.Models.Ortak
{
    public class MenuTanim : BaseModel
    {
        [Required]
        [DisplayName("Menü Adı")]
        public string? Adi { get; set; }
        [DisplayName("Üst Menü")]
        public int UstMenuId { get; set; }
        [ValidateNever]
        [DisplayName("URL")]
        public string? Url { get; set; }
        [ValidateNever]
        [DisplayName("Webpart")]
        public string? Webpart { get; set; }
        [ValidateNever]
        [DisplayName("Sıra")]
        public int Sira { get; set; }

    }
}
