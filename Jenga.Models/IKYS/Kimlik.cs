using Jenga.Models.Sistem;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.IKYS;
public class Kimlik : BaseModel
{
    public int PersonelId { get; set; }
    [ForeignKey("PersonelId")]
    public Personel? Personel { get; set; }
    [DisplayName("TC Kimlik No")]
    public long? TCKimlikNo { get; set; }
    [DisplayName("Baba Adi")]
    public string BabaAdi { get; set; }
    [DisplayName("Anne Adi")]
    public string AnneAdi { get; set; }
    [DisplayName("Dogum Yeri")]
    public string? DogumYeri { get; set; }
    [DisplayName("Dogum Tarihi")]
    public DateTime? DogumTar { get; set; }
    [DisplayName("Medeni hali")]
    public string? MedeniHali { get; set; }
    [DisplayName("Evlilik Tarihi")]
    public DateTime? EvlilikTar { get; set; }
    [DisplayName("Cinsiyet")]
    public string Cinsiyet { get; set; }
    [DisplayName("EskiSoyadi")]
    public string EskiSoyadi { get; set; }
    [DisplayName("KanGrubu")]
    public string KanGrubu { get; set; }
    [DisplayName("Dogumgünü Kutlama")]
    public bool? DogumGunuKutlama { get; set; }
    [DisplayName("Evlilik Kutlama")]
    public bool? EvlilikKutlama { get; set; }

}
