using Jenga.Models.Ortak;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Jenga.Models.MTS;
public class KaynakTanim : BaseModel
{
    [Required]
    [DisplayName("Anı Objesi Kaynağı")]
    public string Adi { get; set; }
    [DisplayName("Açıklama")]
    public string? Aciklama { get; set; }

}
