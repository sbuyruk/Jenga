using Jenga.Models.Sistem;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Jenga.Models.DYS;
public class MalzemeGrubu : BaseModel
{
    [Required]
    [DisplayName("Malzeme Grubu")]
    public string Adi { get; set; }
    
}
