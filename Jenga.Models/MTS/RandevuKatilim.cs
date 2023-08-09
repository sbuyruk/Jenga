using Jenga.Models.Sistem;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Jenga.Models.MTS;
public class RandevuKatilim : BaseModel
{
    [Required]
    [DisplayName("Faaliyet Id")]
    public int RandevuId { get; set; }
    [Required]
    [DisplayName("Katılımcı Id")]
    public int KatilimciId { get; set; }
    [Required]
    [DisplayName("Katılımcı Tipi")]
    public int KatilimciTipi{ get; set; }
   
}
