using Jenga.Models.Sistem;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jenga.Models.TBYS.Kontrol
{
    /// <summary>
    /// Kontrol formunun bir sürümü. Aynı <see cref="Kod"/> altındaki satırlar sürüm zincirini oluşturur.
    /// Yayımlanmış/arşivlenmiş satırların içeriği değiştirilmez; düzenleme yeni taslak sürüm üretir.
    /// </summary>
    [Table("TasinmazKontrolForm_Table")]
    public class TasinmazKontrolForm : BaseModel
    {
        [Required, MaxLength(50)]
        public string Kod { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string Ad { get; set; } = string.Empty;

        public int Surum { get; set; } = 1;

        public KontrolFormKapsami Kapsam { get; set; } = KontrolFormKapsami.Genel;

        /// <summary>Kapsam=Cins ise normalize edilmiş cins (bkz. <see cref="CinsNormalize"/>).</summary>
        [MaxLength(100)]
        public string? KapsamCinsi { get; set; }

        /// <summary>Kapsam=Tasinmaz ise hedef taşınmaz.</summary>
        public int? KapsamTasinmazId { get; set; }

        public KontrolFormDurumu Durum { get; set; } = KontrolFormDurumu.Taslak;

        /// <summary>Kopyalandığı ya da yeni sürümü olduğu form satırı. Kaynak değişiklikleri buraya yansımaz.</summary>
        public int? KaynakFormId { get; set; }

        public int? VarsayilanPeriyotSure { get; set; }
        public PeriyotBirimi? VarsayilanPeriyotBirim { get; set; }

        public string TanimJson { get; set; } = "{}";

        public DateTime? YayimTarihi { get; set; }
        public string? Yayimlayan { get; set; }
        public DateTime? ArsivTarihi { get; set; }
        public string? Arsivleyen { get; set; }

        /// <summary>İyimser eşzamanlılık; her güncellemede servis tarafından artırılır.</summary>
        [ConcurrencyCheck]
        public int KayitSurumu { get; set; }

        [NotMapped]
        public string KapsamEtiketi => Kapsam switch
        {
            KontrolFormKapsami.Cins => $"Cins: {KapsamCinsi}",
            KontrolFormKapsami.Tasinmaz => $"Taşınmaz #{KapsamTasinmazId}",
            _ => "Genel"
        };

        /// <summary>Boşlukları kırpar, Türkçe kültürle büyük harfe çevirir; null/boş → null.</summary>
        public static string? CinsNormalize(string? cins)
        {
            if (string.IsNullOrWhiteSpace(cins)) return null;
            var tr = System.Globalization.CultureInfo.GetCultureInfo("tr-TR");
            return string.Join(' ', cins.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries)).ToUpper(tr);
        }
    }

    /// <summary>Gerçekleştirilen (ya da sürdürülen) bir kontrol. Başlangıçta seçilen kesin form sürümüne bağlıdır.</summary>
    [Table("TasinmazKontrol_Table")]
    public class TasinmazKontrol : BaseModel
    {
        public int TasinmazId { get; set; }
        public int FormId { get; set; }
        public TasinmazKontrolForm? Form { get; set; }

        /// <summary>Kontrol anındaki sorumlu bölge (snapshot).</summary>
        public int? BolgeId { get; set; }
        [MaxLength(200)]
        public string? BolgeAdi { get; set; }

        [MaxLength(500)]
        public string? KontrolEdenler { get; set; }

        /// <summary>Raporu imzalayan makam (adı soyadı); taşınmazın bölgesindeki personelden seçilir.</summary>
        [MaxLength(200)]
        public string? ImzalayanMakam { get; set; }

        /// <summary>Tamamlanma anında üretilir: BölgeKısa-Plaka-Yıl-Sıra (örn. MRM-34-2025-0007).</summary>
        [MaxLength(50)]
        public string? RaporNo { get; set; }

        /// <summary>Fiilî kontrol tarihi; kayıt ve tamamlanma zamanından ayrıdır.</summary>
        public DateTime KontrolTarihi { get; set; }

        public KontrolDurumu Durum { get; set; } = KontrolDurumu.Taslak;
        public DateTime? TamamlanmaTarihi { get; set; }
        public string? Tamamlayan { get; set; }

        /// <summary>Kontrol başlatıldığında taşınmazdan alınan değerlerin anlık kopyası (alan anahtarı → değer).</summary>
        public string TasinmazSnapshotJson { get; set; } = "{}";
        public string CevaplarJson { get; set; } = "{}";

        // --- Periyot / sonraki kontrol izi (tamamlanırken doldurulur) ---
        public int? OncekiPeriyotSure { get; set; }
        public PeriyotBirimi? OncekiPeriyotBirim { get; set; }
        public int? SecilenPeriyotSure { get; set; }
        public PeriyotBirimi? SecilenPeriyotBirim { get; set; }
        public string? PeriyotDegisiklikGerekcesi { get; set; }
        public DateTime? HesaplananSonrakiTarih { get; set; }
        /// <summary>Tek seferlik, gerekçeli elle belirlenmiş tarih (kalıcı periyodu değiştirmez).</summary>
        public DateTime? ElleBelirlenenSonrakiTarih { get; set; }
        public string? TarihDegisiklikGerekcesi { get; set; }
        /// <summary>Tamamlama sırasında güncel plan bu kontrolle güncellendi mi (geçmiş tarihli kontrollerde false olabilir).</summary>
        public bool PlanGuncellendi { get; set; }

        // --- Revizyon / iptal ---
        /// <summary>Bu kayıt, belirtilen tamamlanmış kontrolün gerekçeli revizyonudur.</summary>
        public int? RevizeEdilenKontrolId { get; set; }
        public string? IslemGerekcesi { get; set; }
        public string? IslemYapan { get; set; }
        public DateTime? IslemTarihi { get; set; }

        [ConcurrencyCheck]
        public int KayitSurumu { get; set; }

        [NotMapped]
        public DateTime? SonrakiKontrolTarihi => ElleBelirlenenSonrakiTarih ?? HesaplananSonrakiTarih;
    }

    [Table("TasinmazKontrolEk_Table")]
    public class TasinmazKontrolEk : BaseModel
    {
        public int KontrolId { get; set; }
        [Required, MaxLength(100)]
        public string AlanKodu { get; set; } = string.Empty;
        /// <summary>Tekrarlanabilir grup içindeki ek için satır numarası; aksi hâlde null.</summary>
        public int? SatirNo { get; set; }
        [MaxLength(100)]
        public string? GrupKodu { get; set; }
        [Required, MaxLength(260)]
        public string DosyaAdi { get; set; } = string.Empty;
        [Required, MaxLength(260)]
        public string SaklananAd { get; set; } = string.Empty;
        [MaxLength(100)]
        public string? IcerikTipi { get; set; }
        public long Boyut { get; set; }
    }

    /// <summary>Taşınmaz başına tek güncel kontrol planı. Geçmiş, kontrol kayıtlarındaki periyot izinde saklanır.</summary>
    [Table("TasinmazKontrolPlani_Table")]
    public class TasinmazKontrolPlani : BaseModel
    {
        public int TasinmazId { get; set; }
        public int? PeriyotSure { get; set; }
        public PeriyotBirimi? PeriyotBirim { get; set; }
        public DateTime? SonrakiKontrolTarihi { get; set; }
        /// <summary>Planı son belirleyen tamamlanmış kontrol; gerekçeli elle değişiklikte null olabilir.</summary>
        public int? KaynakKontrolId { get; set; }
        public string? DegisiklikGerekcesi { get; set; }

        [ConcurrencyCheck]
        public int KayitSurumu { get; set; }

        [NotMapped]
        public bool PeriyotTanimli => PeriyotSure is > 0 && PeriyotBirim.HasValue;
    }
}
