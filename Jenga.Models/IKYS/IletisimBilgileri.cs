using Jenga.Models.Sistem;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.IKYS;
public class IletisimBilgileri : BaseModel
{
    public int PersonelId { get; set; }
    [ForeignKey("PersonelId")]
    public Personel? Personel { get; set; }
    [DisplayName("TAdres")]
    public string? Adres { get; set; }
    [DisplayName("Semt")]
    public string? Semt { get; set; }
    [DisplayName("Ili")]
    public int? Ili { get; set; }
    [DisplayName("Ilcesi")]
    public int? Ilcesi { get; set; }
    [DisplayName("Posta Kodu")]
    public string? PostaKodu { get; set; }
    [DisplayName("Dahili")]
    public string? DahiliTelefonu { get; set; }
    [DisplayName("Ev Telefonu")]
    public string? EvTelefonu { get; set; }
    [DisplayName("Cep Telefonu1")]
    public string? CepTelefonu { get; set; }
    [DisplayName("Cep Telefonu2")]
    public string? CepTelefonu2 { get; set; }
    [DisplayName("Intranet E-Posta")]
    public string? IntranetEPosta { get; set; }
    [DisplayName("Internet E-Posta")]
    public string? InternetEPosta { get; set; }
    [DisplayName("Özel E-Posta")]
    public string? OzelEPosta { get; set; }

}
