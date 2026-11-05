using Microsoft.Extensions.Configuration;

namespace Jenga.DataAccess.Services.TBYS
{
    /// <summary>Kontrol eklerinin fiziksel saklama katmanı. Web kökü dışında bir klasör beklenir.</summary>
    public interface IKontrolEkDeposu
    {
        Task<long> KaydetAsync(string saklananAd, Stream icerik, CancellationToken cancellationToken = default);
        Stream? Ac(string saklananAd);
        void Sil(string saklananAd);
    }

    /// <summary>
    /// Dosya sistemi deposu. Kök: <c>TasinmazKontrol:EkKlasoru</c> (yoksa <c>App_Data/TasinmazKontrolEkleri</c>).
    /// Saklanan ad servis tarafından GUID olarak üretilir; yine de yol ayracı/'..' içeremez.
    /// </summary>
    public sealed class DosyaSistemiKontrolEkDeposu : IKontrolEkDeposu
    {
        public const long MaksimumBoyut = 20 * 1024 * 1024;
        private readonly string _kok;

        public DosyaSistemiKontrolEkDeposu(IConfiguration configuration)
        {
            _kok = configuration["TasinmazKontrol:EkKlasoru"]
                   ?? Path.Combine(AppContext.BaseDirectory, "App_Data", "TasinmazKontrolEkleri");
        }

        public async Task<long> KaydetAsync(string saklananAd, Stream icerik, CancellationToken cancellationToken = default)
        {
            Directory.CreateDirectory(_kok);
            var yol = Yol(saklananAd);
            await using var fs = new FileStream(yol, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true);
            await icerik.CopyToAsync(fs, cancellationToken);
            return fs.Length;
        }

        public Stream? Ac(string saklananAd)
        {
            var yol = Yol(saklananAd);
            return File.Exists(yol) ? new FileStream(yol, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true) : null;
        }

        public void Sil(string saklananAd)
        {
            var yol = Yol(saklananAd);
            if (File.Exists(yol)) File.Delete(yol);
        }

        private string Yol(string saklananAd)
        {
            if (string.IsNullOrWhiteSpace(saklananAd) || saklananAd.Contains('/') || saklananAd.Contains('\\') || saklananAd.Contains(".."))
                throw new ArgumentException("Geçersiz dosya adı.", nameof(saklananAd));
            return Path.Combine(_kok, saklananAd);
        }
    }
}
