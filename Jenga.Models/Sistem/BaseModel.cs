using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.Sistem
{
    public partial class BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
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
