using Jenga.Models.Sistem;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
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
    [ValidateNever]
    [DisplayName("Ünvan Kısaltması")]
    public string? KisaAdi { get; set; }

}

