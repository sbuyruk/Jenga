using Jenga.Models.Sistem;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.IKYS;

[Table("UnvanTanim_Table")]
public class UnvanTanim : BaseModel
{
    [Required]
    [DisplayName("Görev Id")]
    public short? GorTipId { get; set; }
    [Required]
    [DisplayName("Ünvan")]
    public string? Adi { get; set; }
    [DisplayName("Ünvan Kisaltmasi")]
    public string? KisaAdi { get; set; }

}

