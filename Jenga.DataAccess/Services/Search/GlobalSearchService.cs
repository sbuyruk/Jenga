using Jenga.DataAccess.Data;
using Jenga.DataAccess.Services.Common;
using Jenga.DataAccess.Services.IKYS;
using Jenga.Models.Enums;
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
        private readonly IServiceAuthorizationContext _authContext;

        public GlobalSearchService(
            IDbContextFactory<ApplicationDbContext> dbFactory,
            ILogService logService,
            IServiceAuthorizationContext authContext)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
            _authContext = authContext ?? throw new ArgumentNullException(nameof(authContext));
        }

        public async Task<Result<GlobalSearchSonucu>> SearchAsync(string query, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Trim().Length < 2)
                return Result<GlobalSearchSonucu>.Success(new GlobalSearchSonucu());

            try
            {
                var canSearchTbys = await _authContext.HasPermissionAsync(ModuleName.TBYS, Operation.View, cancellationToken);
                var canSearchNbys = await _authContext.HasPermissionAsync(ModuleName.NBYS, Operation.View, cancellationToken);
                var canSearchIkys = await _authContext.HasPermissionAsync(ModuleName.IKYS, Operation.View, cancellationToken);
                if (!canSearchTbys && !canSearchNbys && !canSearchIkys)
                    return Result<GlobalSearchSonucu>.Failure(Error.Forbidden(
                        "Arama yapmak için TBYS, NBYS veya İKYS görüntüleme yetkiniz bulunmalıdır.",
                        "Search.View.Forbidden"));

                var regionIds = (await _authContext.GetAuthorizedRegionIdsAsync(cancellationToken)).ToArray();
                var canManageIkys = canSearchIkys &&
                    await _authContext.HasPermissionAsync(ModuleName.IKYS, Operation.Manage, cancellationToken);
                if (regionIds.Length == 0 && !canManageIkys)
                    return Result<GlobalSearchSonucu>.Success(new GlobalSearchSonucu());

                var q = query.Trim();

                // Her modül kendi bağımsız DbContext'ini açar — paralel kullanımda zorunlu
                var tbysTask = canSearchTbys && regionIds.Length > 0
                    ? SearchTbysAsync(_dbFactory, q, regionIds, cancellationToken)
                    : Task.FromResult(new List<SearchGrubu>());
                var nbysTask = canSearchNbys && regionIds.Length > 0
                    ? SearchNbysAsync(_dbFactory, q, regionIds, cancellationToken)
                    : Task.FromResult(new SearchGrubu());
                var ftkTask = canSearchNbys && regionIds.Length > 0
                    ? SearchFtkAsync(_dbFactory, q, regionIds, cancellationToken)
                    : Task.FromResult(new SearchGrubu());
                var ikysTask = canSearchIkys
                    ? SearchIkysAsync(q, regionIds, canManageIkys, cancellationToken)
                    : Task.FromResult(new List<SearchGrubu>());

                await Task.WhenAll(tbysTask, nbysTask, ftkTask, ikysTask);

                var sonuc = new GlobalSearchSonucu();

                var tbysGruplar = tbysTask.Result;
                var nbysGrup   = nbysTask.Result;
                var ftkGrup    = ftkTask.Result;

                sonuc.Gruplar.AddRange(tbysGruplar.Where(g => g.Sonuclar.Count > 0));
                if (nbysGrup.Sonuclar.Count > 0) sonuc.Gruplar.Add(nbysGrup);
                if (ftkGrup.Sonuclar.Count  > 0) sonuc.Gruplar.Add(ftkGrup);
                sonuc.Gruplar.AddRange(ikysTask.Result.Where(g => g.Sonuclar.Count > 0));

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
            IDbContextFactory<ApplicationDbContext> factory, string q, int[] regionIds, CancellationToken ct)
        {
            await using var db = await factory.CreateDbContextAsync(ct);
            var qLow = q.ToLower();
            var provinceIds = db.Il_Table
                .Where(il => il.BolgeId.HasValue && regionIds.Contains(il.BolgeId.Value))
                .Select(il => il.Id);

            var tasinmazlar = await db.Tasinmaz_Table
                .AsNoTracking()
                .Where(t => t.IlId.HasValue && provinceIds.Contains(t.IlId.Value))
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
                .Where(k => k.IlId.HasValue && provinceIds.Contains(k.IlId.Value))
                .Where(k => EF.Functions.Like(k.Adi!.ToLower(),    $"%{qLow}%") ||
                            EF.Functions.Like(k.Soyadi!.ToLower(), $"%{qLow}%"))
                .Take(MaxPerGroup)
                .Select(k => new { k.Id, k.Adi, k.Soyadi, k.Ili, k.Ilcesi, k.KiralamaAmaci })
                .ToListAsync(ct);

            var bagiscilar = await db.TasinmazBagisci_Table
                .AsNoTracking()
                .Where(b => b.IlId.HasValue && provinceIds.Contains(b.IlId.Value))
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
            IDbContextFactory<ApplicationDbContext> factory, string q, int[] regionIds, CancellationToken ct)
        {
            await using var db = await factory.CreateDbContextAsync(ct);
            var qLow = q.ToLower();

            var bagiscilar = await db.NakitBagisci_Table
                .AsNoTracking()
                .Where(b => b.Ili.HasValue && db.Il_Table.Any(il =>
                    il.Id == b.Ili.Value && il.BolgeId.HasValue && regionIds.Contains(il.BolgeId.Value)))
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
            IDbContextFactory<ApplicationDbContext> factory, string q, int[] regionIds, CancellationToken ct)
        {
            await using var db = await factory.CreateDbContextAsync(ct);
            var qLow = q.ToLower();

            var kisiler = await db.FTKKisi_Table
                .AsNoTracking()
                .Where(k => k.Ili.HasValue && db.Il_Table.Any(il =>
                    il.Id == k.Ili.Value && il.BolgeId.HasValue && regionIds.Contains(il.BolgeId.Value)))
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

        private async Task<List<SearchGrubu>> SearchIkysAsync(string q, int[] regionIds, bool canManage, CancellationToken ct)
        {
            var userName = await _authContext.GetUserNameAsync(ct);
            if (string.IsNullOrWhiteSpace(userName)) return [];
            var login = userName.Split('\\').Last().Split('@')[0];
            await using var db = await _dbFactory.CreateDbContextAsync(ct);
            var actorId = await db.Personel_Table.Where(p => p.KullaniciAdi == login)
                .Select(p => (int?)p.Id).SingleOrDefaultAsync(ct);
            if (!actorId.HasValue) return [];
            var pattern = $"%{q.ToLower()}%";
            var units = db.BirimTanim_Table.AsQueryable();
            if (!canManage)
                units = units.Where(b => b.BolgeId.HasValue && regionIds.Contains(b.BolgeId.Value));
            var personIds = db.IsBilgileri_Table.Where(i => i.BirimId.HasValue &&
                units.Select(b => b.Id).Contains(i.BirimId.Value) && i.CalismaDurumu == CalismaDurumu.Calisiyor)
                .Select(i => i.PersonelId);
            var people = db.Personel_Table.AsNoTracking().Where(p => personIds.Contains(p.Id));
            var hasSicil = int.TryParse(q, out var sicil);
            var personnel = await people.Where(p => EF.Functions.Like((p.Adi + " " + p.Soyadi).ToLower(), pattern) ||
                    (hasSicil && p.SicilNo == sicil))
                .OrderBy(p => p.Adi).ThenBy(p => p.Soyadi).Take(MaxPerGroup)
                .Select(p => new { p.Id, p.Adi, p.Soyadi, p.SicilNo }).ToListAsync(ct);
            var approvals = await (from g in GorevOnayService.SearchableApprovals(db, actorId.Value, canManage)
                join p in people on g.PersonelId equals p.Id
                where EF.Functions.Like((p.Adi + " " + p.Soyadi).ToLower(), pattern) ||
                    EF.Functions.Like(g.GorevinSebebi!.ToLower(), pattern) ||
                    EF.Functions.Like(g.GorevinYeri!.ToLower(), pattern) || (hasSicil && p.SicilNo == sicil)
                orderby g.BaslangicTarihi descending, g.Id descending
                select new { g.Id, g.PersonelId, g.GorevinSebebi, g.GorevinYeri, g.BaslangicTarihi, p.Adi, p.Soyadi })
                .Take(MaxPerGroup).ToListAsync(ct);
            var overtimeAccess = await OvertimeService.SearchableEntriesAsync(db, actorId.Value, canManage, ct);
            var overtime = await overtimeAccess.Entries.Where(e => canManage || units.Select(b => b.Id).Contains(e.UnitId))
                .Where(e => EF.Functions.Like(e.FullName.ToLower(), pattern) ||
                    EF.Functions.Like(e.Reason.ToLower(), pattern) || EF.Functions.Like(e.UnitName.ToLower(), pattern) ||
                    (hasSicil && e.SicilNo == sicil))
                .OrderByDescending(e => e.WorkDate).ThenByDescending(e => e.Id).Take(MaxPerGroup)
                .Select(e => new { e.Id, e.PersonelId, e.FullName, e.WorkDate, e.Reason, e.UnitName }).ToListAsync(ct);

            return
            [
                new SearchGrubu
                {
                    Modul = SearchModul.IKYS, GrupBasligi = "Personel", IkonCss = "bi bi-person-badge", RenkCss = "text-primary",
                    Sonuclar = personnel.Select(p => new SearchResultItem
                    {
                        Id = p.Id, Tipi = SearchResultTipi.Personel, Modul = SearchModul.IKYS,
                        Baslik = $"{p.Adi} {p.Soyadi}", AltBaslik = $"Sicil No: {p.SicilNo}",
                        HedefUrl = $"/ikys/personel/{p.Id}"
                    }).ToList()
                },
                new SearchGrubu
                {
                    Modul = SearchModul.IKYS, GrupBasligi = "Görev Onayları", IkonCss = "bi bi-clipboard-check", RenkCss = "text-success",
                    Sonuclar = approvals.Select(g => new SearchResultItem
                    {
                        Id = g.Id, Tipi = SearchResultTipi.GorevOnay, Modul = SearchModul.IKYS,
                        Baslik = $"{g.Adi} {g.Soyadi} — Görev Onayı #{g.Id}",
                        AltBaslik = $"{g.BaslangicTarihi:dd.MM.yyyy} · {g.GorevinYeri}", EkBilgi = g.GorevinSebebi,
                        HedefUrl = $"/ikys/task-approvals?SecilenId={g.Id}" + (g.PersonelId == actorId ? "" : "&Auth=IKYS")
                    }).ToList()
                },
                new SearchGrubu
                {
                    Modul = SearchModul.IKYS, GrupBasligi = "Fazla Mesai", IkonCss = "bi bi-clock-history", RenkCss = "text-warning",
                    Sonuclar = overtime.Select(e => new SearchResultItem
                    {
                        Id = e.Id, Tipi = SearchResultTipi.FazlaMesai, Modul = SearchModul.IKYS,
                        Baslik = $"{e.FullName} — {e.WorkDate:dd.MM.yyyy}", AltBaslik = e.UnitName, EkBilgi = e.Reason,
                        HedefUrl = $"/ikys/overtime?date={e.WorkDate:yyyy-MM-dd}&scope=" +
                            (e.PersonelId == actorId ? "mine" : canManage ? "hr" :
                                overtimeAccess.DelegatedIds.Contains(e.PersonelId) ? "delegated" : "team")
                    }).ToList()
                }
            ];
        }
    }
}
