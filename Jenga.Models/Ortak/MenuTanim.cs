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
