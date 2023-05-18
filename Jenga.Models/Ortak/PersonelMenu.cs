using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jenga.Models.Sistem;
using Jenga.Models.MTS;
using Jenga.Models.IKYS;

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
