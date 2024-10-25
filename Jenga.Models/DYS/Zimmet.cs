using Jenga.Models.IKYS;
using Jenga.Models.Sistem;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Jenga.Models.DYS;
public class Zimmet : BaseModel
{
    [Required]
    [DisplayName("Personel")]
    [ForeignKey("PersonelId")]
    public int PersonelId { get; set; }
    public Personel? Personel { get; set; }
    
    [DisplayName("Malzeme")]
    [ForeignKey("MalzemeId")]
    [ValidateNever]
    public int MalzemeId { get; set; }
    [ValidateNever]
    public Malzeme? Malzeme { get; set; }

    [DisplayName("Adet")]
    [ValidateNever]
    public int Adet { get; set; }

    [ValidateNever]
    public DateTime Tarih { get; set; }
    
}
