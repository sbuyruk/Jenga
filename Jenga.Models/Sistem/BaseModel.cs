using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.Sistem
{
    public partial class BaseModel : IAuditableEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [DisplayName("Açiklama")]
        public string? Aciklama { get; set; }
        public string? Olusturan { get; set; }
        public DateTime? OlusturmaTarihi { get; set; }
        public string? Degistiren { get; set; }
        public DateTime? DegistirmeTarihi { get; set; }
    }
}
