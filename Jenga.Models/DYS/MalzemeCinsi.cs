using Jenga.Models.Sistem;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.DYS;
public class MalzemeCinsi : BaseModel
{
    [Required]
    [DisplayName("Malzeme Cinsi")]
    public string Adi { get; set; }
    [ValidateNever]
    [DisplayName("Üst Malzeme Cinsi")]
    public int UstMalzemeCinsiId { get; set; }
    //public MalzemeCinsi UstMalzemeCinsi { get; set; }

    [ForeignKey("MalzemeGrubuId")]
    [ValidateNever]
    public int MalzemeGrubuId { get; set; }
    [ValidateNever]
    public MalzemeGrubu? MalzemeGrubu { get; set; }

}
