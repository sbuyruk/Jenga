using Jenga.DataAccess.Data;
using Jenga.Models.Search;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.Search
{
    /// <summary>
    /// Tüm modülleri kapsayan global arama servisi.
    /// Yeni modül eklemek için: yeni bir private metot yaz, SearchAsync içinde çağır, 
    /// GlobalSearchSonucu.Gruplar listesine ekle.
    /// </summary>
    public class GlobalSearchService : IGlobalSearchService
    {
        private const string Source = nameof(GlobalSearchService);
        private const int MaxPerGroup = 30;

        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService _logService;

        public GlobalSearchService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        }

        public async Task<Result<GlobalSearchSonucu>> SearchAsync(string query, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Trim().Length < 2)
                return Result<GlobalSearchSonucu>.Success(new GlobalSearchSonucu());

            try
            {
                var q = query.Trim();

                // Her modül kendi bağımsız DbContext'ini açar — paralel kullanımda zorunlu
                var tbysTask  = SearchTbysAsync(_dbFactory, q, cancellationToken);
                var nbysTask  = SearchNbysAsync(_dbFactory, q, cancellationToken);
                var ftkTask   = SearchFtkAsync(_dbFactory, q, cancellationToken);
                // IKYS: bir sonraki adımda eklenecek
                // var ikysTask = SearchIkysAsync(db, q, cancellationToken);

                await Task.WhenAll(tbysTask, nbysTask, ftkTask);

                var sonuc = new GlobalSearchSonucu();

                var tbysGruplar = tbysTask.Result;
                var nbysGrup   = nbysTask.Result;
                var ftkGrup    = ftkTask.Result;

                sonuc.Gruplar.AddRange(tbysGruplar.Where(g => g.Sonuclar.Count > 0));
                if (nbysGrup.Sonuclar.Count > 0) sonuc.Gruplar.Add(nbysGrup);
                if (ftkGrup.Sonuclar.Count  > 0) sonuc.Gruplar.Add(ftkGrup);

                return Result<GlobalSearchSonucu>.Success(sonuc);
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.SearchAsync hata.", ex);
                return Result<GlobalSearchSonucu>.Failure(Error.Unexpected("Arama sırasında hata oluştu.", ex));
            }
        }

        // ──────────────────────────────────────────────────────────────
        // TBYS: Taşınmaz, Kiracı, Taşınmaz Bağışçısı
        // ──────────────────────────────────────────────────────────────
        private static async Task<List<SearchGrubu>> SearchTbysAsync(
            IDbContextFactory<ApplicationDbContext> factory, string q, CancellationToken ct)
        {
            await using var db = await factory.CreateDbContextAsync(ct);
            var qLow = q.ToLower();

            var tasinmazlar = await db.Tasinmaz_Table
                .AsNoTracking()
                .Where(t => t.EnvanterdeMi == 1 &&
                    (EF.Functions.Like(t.Ili!.ToLower(),          $"%{qLow}%") ||
                     EF.Functions.Like(t.Ilcesi!.ToLower(),       $"%{qLow}%") ||
                     EF.Functions.Like(t.Adres!.ToLower(),        $"%{qLow}%") ||
                     EF.Functions.Like(t.Cinsi!.ToLower(),        $"%{qLow}%") ||
                     EF.Functions.Like(t.EmlakSicilNo!.ToLower(), $"%{qLow}%") ||
                     EF.Functions.Like(t.Nitelik!.ToLower(),      $"%{qLow}%")))
                .Take(MaxPerGroup)
                .Select(t => new { t.Id, t.Cinsi, t.EmlakSicilNo, t.Ili, t.Ilcesi, t.Adres })
                .ToListAsync(ct);

            var kiracijlar = await db.Kiraci_Table
                .AsNoTracking()
                .Where(k => EF.Functions.Like(k.Adi!.ToLower(),    $"%{qLow}%") ||
                            EF.Functions.Like(k.Soyadi!.ToLower(), $"%{qLow}%"))
                .Take(MaxPerGroup)
                .Select(k => new { k.Id, k.Adi, k.Soyadi, k.Ili, k.Ilcesi, k.KiralamaAmaci })
                .ToListAsync(ct);

            var bagiscilar = await db.TasinmazBagisci_Table
                .AsNoTracking()
                .Where(b => b.Gizli != true &&
                           (EF.Functions.Like(b.Adi.ToLower(),     $"%{qLow}%") ||
                            EF.Functions.Like(b.Soyadi!.ToLower(), $"%{qLow}%")))
                .Take(MaxPerGroup)
                .Select(b => new { b.Id, b.Adi, b.Soyadi, b.Ili, b.Ilcesi, b.Meslegi })
                .ToListAsync(ct);

            return
            [
                new SearchGrubu
                {
                    Modul       = SearchModul.TBYS,
                    GrupBasligi = "Taşınmazlar",
                    IkonCss     = "bi bi-house-door-fill",
                    RenkCss     = "text-primary",
                    Sonuclar    = tasinmazlar.Select(t => new SearchResultItem
                    {
                        Id       = t.Id,
                        Tipi     = SearchResultTipi.Tasinmaz,
                        Modul    = SearchModul.TBYS,
                        Baslik   = $"{t.Cinsi ?? "Taşınmaz"} — {t.EmlakSicilNo ?? t.Id.ToString()}",
                        AltBaslik = $"{t.Ili}, {t.Ilcesi}",
                        EkBilgi  = t.Adres,
                        HedefUrl = $"/tbys/tasinmaz/{t.Id}"
                    }).ToList()
                },
                new SearchGrubu
                {
                    Modul       = SearchModul.TBYS,
                    GrupBasligi = "Kiracılar",
                    IkonCss     = "bi bi-person-fill",
                    RenkCss     = "text-success",
                    Sonuclar    = kiracijlar.Select(k => new SearchResultItem
                    {
                        Id        = k.Id,
                        Tipi      = SearchResultTipi.Kiraci,
                        Modul     = SearchModul.TBYS,
                        Baslik    = $"{k.Adi} {k.Soyadi}".Trim(),
                        AltBaslik = $"{k.Ili ?? ""} {k.Ilcesi ?? ""}".Trim(),
                        EkBilgi   = k.KiralamaAmaci,
                        HedefUrl  = $"/tbys/kiraci/{k.Id}"
                    }).ToList()
                },
                new SearchGrubu
                {
                    Modul       = SearchModul.TBYS,
                    GrupBasligi = "Taşınmaz Bağışçıları",
                    IkonCss     = "bi bi-gift-fill",
                    RenkCss     = "text-warning",
                    Sonuclar    = bagiscilar.Select(b => new SearchResultItem
                    {
                        Id        = b.Id,
                        Tipi      = SearchResultTipi.TasinmazBagisci,
                        Modul     = SearchModul.TBYS,
                        Baslik    = $"{b.Adi} {b.Soyadi}".Trim(),
                        AltBaslik = $"{b.Ili ?? ""} {b.Ilcesi ?? ""}".Trim(),
                        EkBilgi   = b.Meslegi,
                        HedefUrl  = $"/tbys/bagisci/{b.Id}"
                    }).ToList()
                },
            ];
        }

        // ──────────────────────────────────────────────────────────────
        // NBYS: Nakit Bağışçı
        // ──────────────────────────────────────────────────────────────
        private static async Task<SearchGrubu> SearchNbysAsync(
            IDbContextFactory<ApplicationDbContext> factory, string q, CancellationToken ct)
        {
            await using var db = await factory.CreateDbContextAsync(ct);
            var qLow = q.ToLower();

            var bagiscilar = await db.NakitBagisci_Table
                .AsNoTracking()
                .Where(b => EF.Functions.Like(b.Adi!.ToLower(),    $"%{qLow}%") ||
                            EF.Functions.Like(b.Soyadi!.ToLower(), $"%{qLow}%"))
                .Take(MaxPerGroup)
                .Select(b => new { b.Id, b.Adi, b.Soyadi })
                .ToListAsync(ct);

            return new SearchGrubu
            {
                Modul       = SearchModul.NBYS,
                GrupBasligi = "Nakit Bağışçıları",
                IkonCss     = "bi bi-cash-coin",
                RenkCss     = "text-info",
                Sonuclar    = bagiscilar.Select(b => new SearchResultItem
                {
                    Id        = b.Id,
                    Tipi      = SearchResultTipi.NakitBagisci,
                    Modul     = SearchModul.NBYS,
                    Baslik    = $"{b.Adi} {b.Soyadi}".Trim(),
                    AltBaslik = "NBYS",
                    HedefUrl  = $"/nbys/bagisci/{b.Id}"
                }).ToList()
            };
        }

        // ──────────────────────────────────────────────────────────────
        // FTK: FTK Kişisi
        // ──────────────────────────────────────────────────────────────
        private static async Task<SearchGrubu> SearchFtkAsync(
            IDbContextFactory<ApplicationDbContext> factory, string q, CancellationToken ct)
        {
            await using var db = await factory.CreateDbContextAsync(ct);
            var qLow = q.ToLower();

            var kisiler = await db.FTKKisi_Table
                .AsNoTracking()
                .Where(k => EF.Functions.Like(k.Adi!.ToLower(),    $"%{qLow}%") ||
                            EF.Functions.Like(k.Soyadi!.ToLower(), $"%{qLow}%"))
                .Take(MaxPerGroup)
                .Select(k => new { k.Id, k.Adi, k.Soyadi })
                .ToListAsync(ct);

            return new SearchGrubu
            {
                Modul       = SearchModul.FTK,
                GrupBasligi = "FTK Kişileri",
                IkonCss     = "bi bi-people-fill",
                RenkCss     = "text-danger",
                Sonuclar    = kisiler.Select(k => new SearchResultItem
                {
                    Id        = k.Id,
                    Tipi      = SearchResultTipi.FtkKisi,
                    Modul     = SearchModul.FTK,
                    Baslik    = $"{k.Adi} {k.Soyadi}".Trim(),
                    AltBaslik = "FTK",
                    HedefUrl  = $"/ftk/kisi/{k.Id}"
                }).ToList()
            };
        }

        // ──────────────────────────────────────────────────────────────
        // IKYS: Personel — bir sonraki adımda eklenecek
        // ──────────────────────────────────────────────────────────────
        // private static async Task<SearchGrubu> SearchIkysAsync(
        //     ApplicationDbContext db, string q, CancellationToken ct) { ... }
    }
}
