using Jenga.Models.Sistem;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.IKYS;

public class IzinDonem : BaseModel
{
    [ValidateNever]
    public int? PersonelId { get; set; }
    [ForeignKey("PersonelId")]
    [ValidateNever]
    public Personel? Personel { get; set; }
    [DisplayName("Başlangıç Tarihi")]
    public DateTime? BaslangicTarihi { get; set; }
    [DisplayName("Bitiş Tarihi")]
    public DateTime? BitisTarihi { get; set; }
    [DisplayName("Adı")]
    public string? Adi { get; set; }
    [DisplayName("İzin Tipi")]
    public int? IzinTipi { get; set; }
    [DisplayName("İzin Hakkı")]
    public string? IzinHakki { get; set; }
    [DisplayName("Kullanılan İzin")]
    public string? KullanilanIzin { get; set; }
    [DisplayName("Kalan İzin")]
    public string? KalanIzin { get; set; }
    [DisplayName("Birim")]
    public string? Birim { get; set; }
}
