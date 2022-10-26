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
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Jenga.Models.Ortak
{
    public class MenuTanim : BaseModel
    {
        [Required]
        [DisplayName("Menu Başlığı")]
        public string? Adi { get; set; }
        public int UstMenuId { get; set; }
        [ValidateNever]
        public string? Aciklama { get; set; }


    }
}
