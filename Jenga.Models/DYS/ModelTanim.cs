using Jenga.Models.Sistem;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.DYS;
public class ModelTanim : BaseModel
{
    [Required]
    [DisplayName("Model Tanımı")]

    public string Adi { get; set; }
    [DisplayName("Marka Tanımı")]
    public int MarkaTanimId { get; set; }
    [ForeignKey("MarkaTanimId")]
    [ValidateNever]
    public MarkaTanim? MarkaTanim { get; set; }

}
