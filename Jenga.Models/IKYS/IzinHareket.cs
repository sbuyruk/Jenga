using Jenga.Models.Sistem;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.IKYS;

public class IzinHareket : BaseModel
{
    public int? PersonelId { get; set; }
    [ForeignKey("PersonelId")]
    public Personel? Personel { get; set; }
    [DisplayName("İzin Talep Id")]
    public int? IzinTalepId { get; set; }
    [DisplayName("İzin Dönem Id")]
    public int? IzinDonemId { get; set; }
    [DisplayName("İzin Tipi")]
    public int? IzinTipi { get; set; }
    [DisplayName("Başlangıç Tarihi")]
    public DateTime? BaslangicTarihi { get; set; }
    [DisplayName("Bitiş Tarihi")]
    public DateTime? BitisTarihi { get; set; }
    [DisplayName("Süre")]
    public string? Sure { get; set; }
    [DisplayName("Birim")]
    public string? Birim { get; set; }
    [DisplayName("Adres")]
    public string? Adres { get; set; }
    [DisplayName("Vekil İmza")]
    public int? VekilImza { get; set; }
    [DisplayName("Amir İmza")]
    public int? AmirImza { get; set; }
    [DisplayName("Onay İmza")]
    public int? OnayImza { get; set; }
    [DisplayName("Mahsup")]
    public bool? Mahsup { get; set; }
    [DisplayName("Önceki İzin")]
    public string? OncekiIzinStr { get; set; }
    [DisplayName("Kullanılan İzin")]
    public string? KullanilanIzinStr { get; set; }
    [DisplayName("Kalan İzin")]
    public string? KalanIzinStr { get; set; }
}
