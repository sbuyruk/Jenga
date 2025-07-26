using Jenga.Models.IKYS;
using Jenga.Models.Sistem;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.Ortak
{
    public class PersonelMenu : BaseModel
    {
        public int PersonelId { get; set; }
        [DisplayName("Personel")]
        [ForeignKey("PersonelId")]
        [ValidateNever]
        public Personel Personel { get; set; }
        public int MenuTanimId { get; set; }
        [DisplayName("Menü")]
        [ForeignKey("MenuTanimId")]
        [ValidateNever]
        public MenuTanim? MenuTanim { get; set; }
    }
}
