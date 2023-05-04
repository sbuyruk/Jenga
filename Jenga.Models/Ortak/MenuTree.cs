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
    public class MenuTree 
    {
        [Required]
        public int id { get; set; }
        public string? text { get; set; }
        public string? url { get; set; }
        [ValidateNever]
        public string? webpart { get; set; }
        [ValidateNever]
        public int sira { get; set; }
        [ValidateNever]
        public string? aciklama { get; set; }
        [ValidateNever]
        public List<MenuTree>? nodes { get; set; }

    }
}
