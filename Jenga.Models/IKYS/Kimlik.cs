using Jenga.Models.Sistem;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.IKYS;
public class Kimlik : BaseModel
{
    [ValidateNever]
    public int PersonelId { get; set; }
    [ForeignKey("PersonelId")]
    [ValidateNever]
    public Personel? Personel { get; set; }
    [ValidateNever]
    [DisplayName("TC Kimlik No")]
    public long? TCKimlikNo { get; set; }
    [ValidateNever]
    [DisplayName("Baba Adı")]
    public string BabaAdi { get; set; }
    [ValidateNever]
    [DisplayName("Anne Adı")]
    public string AnneAdi { get; set; }
    [ValidateNever]
    [DisplayName("Doğum Yeri")]
    public string? DogumYeri { get; set; }
    [ValidateNever]
    [DisplayName("Doğum Tarihi")]
    public DateTime? DogumTar { get; set; }
    [ValidateNever]
    [DisplayName("Medeni hali")]
    public string? MedeniHali { get; set; }
    [ValidateNever]
    [DisplayName("Evlilik Tarihi")]
    public DateTime? EvlilikTar { get; set; }
    [ValidateNever]
    [DisplayName("Cinsiyet")]
    public string Cinsiyet { get; set; }
    [ValidateNever]
    [DisplayName("EskiSoyadi")]
    public string EskiSoyadi { get; set; }
    [ValidateNever]
    [DisplayName("KanGrubu")]
    public string KanGrubu { get; set; }
    [ValidateNever]
    [DisplayName("Doğumgünü Kutlama")]
    public bool DogumGunuKutlama { get; set; }
    [ValidateNever]
    [DisplayName("Evlilik Kutlama")]
    public bool EvlilikKutlama { get; set; }

}
