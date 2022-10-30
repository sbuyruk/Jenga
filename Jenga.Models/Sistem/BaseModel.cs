using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jenga.Models.Sistem
{
    public partial class BaseModel
    {
        [Key]
        public int Id { get; set; }
        [ValidateNever]
        public string? Olusturan { get; set; }
        [ValidateNever]
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        [ValidateNever]
        public string? Degistiren { get; set; }
        [ValidateNever]
        public DateTime DegistirmeTarihi { get; set; } = DateTime.Now;
    }
}
