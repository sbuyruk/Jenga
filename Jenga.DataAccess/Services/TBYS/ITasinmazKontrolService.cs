using Jenga.Models.TBYS.Kontrol;
using Jenga.Utility.Results;

namespace Jenga.DataAccess.Services.TBYS
{
    /// <summary>Liste satırı: kontrol + taşınmaz özet bilgisi.</summary>
    public sealed class TasinmazKontrolOzet
    {
        public int Id { get; set; }
        public int TasinmazId { get; set; }
        public string? EmlakSicilNo { get; set; }
        public string? Cinsi { get; set; }
        public string? Ili { get; set; }
        public string? Ilcesi { get; set; }
        public string? BolgeAdi { get; set; }
        public string FormAd { get; set; } = string.Empty;
        public int FormSurum { get; set; }
        public DateTime KontrolTarihi { get; set; }
        public KontrolDurumu Durum { get; set; }
        public string? KontrolEdenler { get; set; }
        public string? RaporNo { get; set; }
        public DateTime? SonrakiKontrolTarihi { get; set; }
        public int? RevizeEdilenKontrolId { get; set; }
    }

    /// <summary>Taşınmazın bölgesindeki personel; birim amiri <see cref="Amir"/>=true ile işaretlenir.</summary>
    public sealed class BolgePersoneli
    {
        public int PersonelId { get; set; }
        public string AdSoyad { get; set; } = string.Empty;
        public string? Unvan { get; set; }
        public bool Amir { get; set; }
    }

    /// <summary>Planlama listesi satırı: taşınmaz + güncel plan + son kontrol.</summary>
    public sealed class TasinmazPlanOzet
    {
        public int TasinmazId { get; set; }
        public string? EmlakSicilNo { get; set; }
        public string? Adres { get; set; }
        public string? AdaNo { get; set; }
        public string? ParselNo { get; set; }
        public string? Cinsi { get; set; }
        public string? Ili { get; set; }
        public string? Ilcesi { get; set; }
        public string? BolgeAdi { get; set; }
        public int? PeriyotSure { get; set; }
        public PeriyotBirimi? PeriyotBirim { get; set; }
        public DateTime? SonrakiKontrolTarihi { get; set; }
        public DateTime? SonKontrolTarihi { get; set; }
        public int? SonKontrolId { get; set; }
        /// <summary>Taşınmaz için açık (tamamlanmamış) taslak kontrol varsa Id'si.</summary>
        public int? AcikTaslakId { get; set; }
        public bool Gecikmis => SonrakiKontrolTarihi.HasValue && SonrakiKontrolTarihi.Value.Date < DateTime.Today;
    }

    public sealed class KontrolBaslatIstegi
    {
        public int TasinmazId { get; set; }
        public int FormId { get; set; }
        public DateTime KontrolTarihi { get; set; } = DateTime.Today;
        public string? KontrolEdenler { get; set; }
        public string? ImzalayanMakam { get; set; }
        /// <summary>Doluysa yeni kayıt, belirtilen tamamlanmış kontrolün revizyonu olarak açılır.</summary>
        public int? RevizeEdilenKontrolId { get; set; }
        public string? RevizyonGerekcesi { get; set; }
    }

    public sealed class KontrolTamamlaIstegi
    {
        public int KontrolId { get; set; }
        public int KayitSurumu { get; set; }
        public DateTime KontrolTarihi { get; set; }
        public string? KontrolEdenler { get; set; }
        public string? ImzalayanMakam { get; set; }
        public KontrolCevaplari Cevaplar { get; set; } = new();
        /// <summary>Kalıcı periyot; mevcut plandan farklıysa gerekçe zorunlu.</summary>
        public int? PeriyotSure { get; set; }
        public PeriyotBirimi? PeriyotBirim { get; set; }
        public string? PeriyotDegisiklikGerekcesi { get; set; }
        /// <summary>Tek seferlik elle tarih; doluysa gerekçe zorunlu.</summary>
        public DateTime? ElleBelirlenenSonrakiTarih { get; set; }
        public string? TarihDegisiklikGerekcesi { get; set; }
    }

    public sealed class PlanGuncelleIstegi
    {
        public int TasinmazId { get; set; }
        public int? PeriyotSure { get; set; }
        public PeriyotBirimi? PeriyotBirim { get; set; }
        public DateTime? SonrakiKontrolTarihi { get; set; }
        public string Gerekce { get; set; } = string.Empty;
    }

    public sealed class EkYukleIstegi
    {
        public int KontrolId { get; set; }
        public string AlanKodu { get; set; } = string.Empty;
        public string? GrupKodu { get; set; }
        public int? SatirNo { get; set; }
        public string DosyaAdi { get; set; } = string.Empty;
        public string? IcerikTipi { get; set; }
        public Stream Icerik { get; set; } = Stream.Null;
    }

    /// <summary>
    /// Taşınmaz kontrollerinin yürütülmesi, planlanması ve ekleri. Tüm işlemler çağıranın
    /// yetkili bölgeleriyle sunucu tarafında sınırlanır (Il.BolgeId üzerinden).
    /// </summary>
    public interface ITasinmazKontrolService
    {
        Task<Result<List<TasinmazKontrolOzet>>> GetListeAsync(int? tasinmazId = null, KontrolDurumu? durum = null, CancellationToken cancellationToken = default);
        Task<Result<TasinmazKontrol>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<List<TasinmazKontrolEk>>> GetEklerAsync(int kontrolId, CancellationToken cancellationToken = default);
        Task<Result<TasinmazKontrolEk>> GetEkAsync(int ekId, CancellationToken cancellationToken = default);
        Task<Result<List<TasinmazPlanOzet>>> GetPlanListesiAsync(bool sadeceGecikmis = false, CancellationToken cancellationToken = default);
        Task<Result<TasinmazKontrolPlani?>> GetPlanAsync(int tasinmazId, CancellationToken cancellationToken = default);
        /// <summary>Taşınmazın bölgesindeki çalışan personel; bölge birimlerinin amirleri başta gelir.</summary>
        Task<Result<List<BolgePersoneli>>> GetBolgePersoneliAsync(int tasinmazId, CancellationToken cancellationToken = default);

        /// <summary>Taslak kontrol açar; taşınmaz snapshot'ı ve başlangıç değerlerini doldurur.</summary>
        Task<Result<int>> BaslatAsync(KontrolBaslatIstegi istek, CancellationToken cancellationToken = default);

        /// <summary>Taslak cevaplarını kaydeder (zorunluluk denetlenmez, tipler denetlenir).</summary>
        Task<Result> TaslakKaydetAsync(int kontrolId, int kayitSurumu, KontrolCevaplari cevaplar, string? kontrolEdenler, DateTime kontrolTarihi, string? imzalayanMakam = null, CancellationToken cancellationToken = default);

        /// <summary>Doğrular, tamamlar ve güncel planı aynı işlemde günceller.</summary>
        Task<Result> TamamlaAsync(KontrolTamamlaIstegi istek, CancellationToken cancellationToken = default);

        Task<Result> IptalEtAsync(int kontrolId, string gerekce, CancellationToken cancellationToken = default);

        /// <summary>Taslak kontrolü kalıcı siler (ekleriyle birlikte).</summary>
        Task<Result> DeleteAsync(int kontrolId, CancellationToken cancellationToken = default);

        /// <summary>Kontrolden bağımsız gerekçeli plan düzenlemesi.</summary>
        Task<Result> PlanGuncelleAsync(PlanGuncelleIstegi istek, CancellationToken cancellationToken = default);

        Task<Result<int>> EkYukleAsync(EkYukleIstegi istek, CancellationToken cancellationToken = default);
        Task<Result> EkSilAsync(int ekId, CancellationToken cancellationToken = default);
        /// <summary>Yetki ve bölge denetimi sonrası dosya içeriğini açar.</summary>
        Task<Result<(Stream Icerik, TasinmazKontrolEk Ek)>> EkAcAsync(int ekId, CancellationToken cancellationToken = default);
    }
}
