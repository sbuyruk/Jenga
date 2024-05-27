using Jenga.Models.MTS;
using Jenga.Models.Sistem;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.IKYS;
public class ResmiTatil : BaseModel
{
    [ValidateNever]
    [DisplayName("Gün")]
    public int Gun { get; set; }
    [ValidateNever]
    [DisplayName("Ay")]
    public int Ay { get; set; }
    [ValidateNever]
    [DisplayName("Yil")]
    public int Yil { get; set; }
    [ValidateNever]
    [DisplayName("Tatil")]
    public string Tatil { get; set; }
    [ValidateNever]
    [DisplayName("Başlama Tarihi")]
    public DateTime BaslamaTarihi { get; set; }
    [ValidateNever]
    [DisplayName("Bitiş Tarihi")]
    public DateTime BitisTarihi { get; set; }
    [ValidateNever]
    [DisplayName("İlan Tarihi")]
    public DateTime? IlanTarihi { get; set; }
    [ValidateNever]
    [DisplayName("İptal Tarihi")]
    public DateTime? IptalTarihi { get; set; }

}
