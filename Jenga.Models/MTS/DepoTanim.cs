using Jenga.Models.Sistem;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Jenga.Models.MTS;
public class DepoTanim : BaseModel
{
    [Required]
    [DisplayName("Depo Adı")]
    public string Adi { get; set; }
    [DisplayName("Açıklama")]
    public string? Aciklama { get; set; }
}
