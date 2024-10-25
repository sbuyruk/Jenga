using Jenga.Models.Sistem;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Jenga.Models.DYS;
public class Malzeme : BaseModel
{
    [Required]
    [DisplayName("Malzeme")]
    public string Adi { get; set; }
 
    [ForeignKey("MalzemeCinsiId")]
    [ValidateNever]
    public int MalzemeCinsiId { get; set; }
    [ValidateNever]
    public MalzemeCinsi? MalzemeCinsi { get; set; }
    [ForeignKey("MarkaTanimId")]
    [ValidateNever]
    public int MarkaTanimId { get; set; }
    [ValidateNever]
    public MarkaTanim? MarkaTanim { get; set; }
    [ForeignKey("ModelTanimId")]
    [ValidateNever]
    public int ModelTanimId { get; set; }
    [ValidateNever]
    public ModelTanim? ModelTanim { get; set; }
    [JsonIgnore]
    public ICollection<MalzemeHareket> MalzemeHarekets { get; set; } = new List<MalzemeHareket>();
}
