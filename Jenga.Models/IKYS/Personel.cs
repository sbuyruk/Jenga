using Jenga.Models.Enums;
using Jenga.Models.Sistem;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.IKYS;
public class Personel : BaseModel
{
    [Required]
    [DisplayName("Adi")]
    public string Adi { get; set; }
    [Required]
    [DisplayName("Soyadi")]
    public string Soyadi { get; set; }
    [Required]
    [DisplayName("Sicil No")]
    public int SicilNo { get; set; }
    [DisplayName("Tahsili")]
    public int? Tahsili { get; set; }
    [DisplayName("Kullanici Adi")]
    public string? KullaniciAdi { get; set; }
    [DisplayName("Asker/Sivil")]
    [Column("Asker_sivil")]
    public AskerSivil? AskerSivil { get; set; }
    [DisplayName("Tipi")]
    public PersonelTipi? Tipi { get; set; }
    public Kimlik Kimlik { get; set; }
    public IletisimBilgileri IletisimBilgileri { get; set; }
    public IsBilgileri IsBilgileri { get; set; }
    }
