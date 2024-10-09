using Jenga.Models.Sistem;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Jenga.Models.DYS;
public class EnvanterTanim : BaseModel
{
    [Required]
    [DisplayName("Envanter Tanımı")]
    public string Adi { get; set; }

}
