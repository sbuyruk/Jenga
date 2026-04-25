using Jenga.Models.Sistem;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;

namespace Jenga.Models.IKYS;

public class EgitimSeviyesi : BaseModel
{
    [DisplayName("Adı")]
    public string? Adi { get; set; }
    [DisplayName("Kısa Adı")]
    public string? KisaAdi { get; set; }
}
