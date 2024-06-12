using Jenga.Models.Sistem;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Jenga.Models.MTS;
public class FaaliyetAmaci : BaseModel
{
    [Required]
    [DisplayName("Faaliyet Amacı")]
    public string Adi { get; set; }
}
