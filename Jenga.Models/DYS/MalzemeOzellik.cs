using Jenga.Models.Sistem;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.DYS;
public class MalzemeOzellik : BaseModel
{
    [Required]
    [ForeignKey("MalzemeId")]
    public int MalzemeId { get; set; }
    [ValidateNever]
    public Malzeme? Malzeme { get; set; }
    [Required]
    [ForeignKey("OzellikId")]
    public int OzellikId { get; set; }
    [ValidateNever]
    public Ozellik? Ozellik { get; set; }

}
