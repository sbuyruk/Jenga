using Jenga.Models.Sistem;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.DYS;
public class Ozellik : BaseModel
{
    [Required]
    [DisplayName("Malzeme Özelliği")]
    public string Adi { get; set; }
    [ValidateNever]
    public string OlcuBirimi { get; set; }
    [ValidateNever]
    public string Miktar { get; set; }
    [ValidateNever]
    [ForeignKey("MalzemeCinsiId")]
    public int MalzemeCinsiId { get; set; }
    [ValidateNever]
    public MalzemeCinsi? MalzemeCinsi { get; set; }
}
