namespace Jenga.Models.Sistem;

/// <summary>
/// Oluşturma ve değiştirme bilgilerini otomatik doldurmak için işaretleyici arayüz.
/// <see cref="BaseModel"/> bu arayüzü implemente eder;
/// <c>ApplicationDbContext.SaveChangesAsync</c> override'ı tüm entity'leri
/// bu arayüz üzerinden bularak audit alanlarını merkezi olarak doldurur.
/// </summary>
public interface IAuditableEntity
{
    string? Olusturan { get; set; }
    DateTime? OlusturmaTarihi { get; set; }
    string? Degistiren { get; set; }
    DateTime? DegistirmeTarihi { get; set; }
}
