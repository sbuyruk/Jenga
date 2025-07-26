using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Jenga.Models.Sistem
{
    public partial class BaseModel
    {
        [Key]
        public int Id { get; set; }
        [ValidateNever]
        [DisplayName("Açıklama")]
        public string? Aciklama { get; set; }
        [ValidateNever]
        public string? Olusturan { get; set; }
        [ValidateNever]
        public DateTime? OlusturmaTarihi { get; set; }
        [ValidateNever]
        public string? Degistiren { get; set; }
        [ValidateNever]
        public DateTime? DegistirmeTarihi { get; set; } = DateTime.Now;
    }
}
