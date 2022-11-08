using Jenga.Models.Sistem;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Jenga.Models.Ortak;
public class ModulTanim : BaseModel
{
    [Required]
    [DisplayName("Modül Adı")]
    public string? Adi { get; set; }
    [ValidateNever]
    [DisplayName("Program Adı")]
    public string? ProgramAdi { get; set; }
    [ValidateNever]
    [DisplayName("Webpart Adı")]
    public string? WebpartAdi { get; set; }
}
