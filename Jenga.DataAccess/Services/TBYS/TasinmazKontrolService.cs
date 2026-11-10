using Jenga.DataAccess.Data;
using Jenga.DataAccess.Services.Common;
using Jenga.Models.Enums;
using Jenga.Models.TBYS;
using Jenga.Models.TBYS.Kontrol;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.TBYS
{
    public class TasinmazKontrolService : ServiceBase, ITasinmazKontrolService
    {
        private const string Source = nameof(TasinmazKontrolService);
        private const string BolgeYetkisiYok = "Bu taşınmaz yetkili olduğunuz bölgelerde değil.";
        private const string EszamanlilikMesaji = "Kayıt başka bir kullanıcı tarafından değiştirildi. Sayfayı yenileyin.";

        private static readonly HashSet<string> IzinliUzantilar = new(StringComparer.OrdinalIgnoreCase)
            { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".pdf", ".doc", ".docx", ".xls", ".xlsx" };

        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly IDbContextScopeFactory _scopeFactory;
        private readonly IKontrolEkDeposu _ekDeposu;
        private readonly IServiceAuthorizationContext? _authContext;
        private readonly ILogService _logService;

        public TasinmazKontrolService(
            IDbContextFactory<ApplicationDbContext> dbFactory,
            IDbContextScopeFactory scopeFactory,
            IKontrolEkDeposu ekDeposu,
            ILogService logService,
            IServiceAuthorizationContext? authContext = null) : base(authContext)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _ekDeposu = ekDeposu ?? throw new ArgumentNullException(nameof(ekDeposu));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
            _authContext = authContext;
        }

        // ----------------------------------------------------------------- Bölge yetkisi

        /// <summary>null → kısıt yok (auth context yok); boş liste → hiçbir bölge.</summary>
        private async Task<IReadOnlyList<int>?> YetkiliBolgelerAsync(CancellationToken ct)
            => _authContext is null ? null : await _authContext.GetAuthorizedRegionIdsAsync(ct);

        private static IQueryable<Tasinmaz> BolgeyeGoreFiltrele(ApplicationDbContext db, IReadOnlyList<int>? bolgeler)
        {
            var q = db.Tasinmaz_Table.AsNoTracking();
            if (bolgeler is null) return q;
            if (bolgeler.Count == 0) return q.Where(_ => false);
            return q.Where(t => db.Il_Table.Any(il => il.Id == t.IlId && il.BolgeId != null && bolgeler.Contains(il.BolgeId.Value)));
        }

        private async Task<(Tasinmaz Tasinmaz, int? BolgeId, string? BolgeAdi)?> YetkiliTasinmazAsync(ApplicationDbContext db, int tasinmazId, CancellationToken ct)
        {
            var bolgeler = await YetkiliBolgelerAsync(ct);
            var satir = await BolgeyeGoreFiltrele(db, bolgeler)
                .Where(t => t.Id == tasinmazId)
                .Select(t => new
                {
                    Tasinmaz = t,
                    Bolge = db.Il_Table.Where(il => il.Id == t.IlId)
                        .Select(il => db.Bolge_Table.Where(b => b.Id == il.BolgeId).Select(b => new { b.Id, b.Adi }).FirstOrDefault())
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync(ct);
            return satir is null ? null : (satir.Tasinmaz, satir.Bolge?.Id, satir.Bolge?.Adi);
        }

        private async Task<bool> KontroleErisebilirAsync(ApplicationDbContext db, int tasinmazId, CancellationToken ct)
        {
            var bolgeler = await YetkiliBolgelerAsync(ct);
            return await BolgeyeGoreFiltrele(db, bolgeler).AnyAsync(t => t.Id == tasinmazId, ct);
        }

        // ----------------------------------------------------------------- Sorgular

        public async Task<Result<List<BolgePersoneli>>> GetBolgePersoneliAsync(int tasinmazId, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var yetkili = await YetkiliTasinmazAsync(db, tasinmazId, cancellationToken);
                if (yetkili is null) return Result<List<BolgePersoneli>>.Failure(Error.Forbidden(BolgeYetkisiYok));
                var bolgeId = yetkili.Value.BolgeId;
                if (bolgeId is null) return Result<List<BolgePersoneli>>.Success(new());

                var birimler = db.BirimTanim_Table.AsNoTracking().Where(b => b.BolgeId == bolgeId && b.Aktif != false);
                var amirIdler = await birimler.Where(b => b.AmirId != null).Select(b => b.AmirId!.Value).Distinct().ToListAsync(cancellationToken);

                var liste = await db.IsBilgileri_Table.AsNoTracking()
                    .Where(i => i.CalismaDurumu == CalismaDurumu.Calisiyor
                                && ((i.BirimId != null && birimler.Any(b => b.Id == i.BirimId)) || amirIdler.Contains(i.PersonelId)))
                    .Select(i => new BolgePersoneli
                    {
                        PersonelId = i.PersonelId,
                        AdSoyad = (i.Personel.Adi + " " + i.Personel.Soyadi).Trim(),
                        Unvan = i.UnvanTanim != null ? i.UnvanTanim.Adi : null,
                        Amir = amirIdler.Contains(i.PersonelId)
                    })
                    .ToListAsync(cancellationToken);

                liste = liste.GroupBy(p => p.PersonelId).Select(g => g.First())
                    .OrderByDescending(p => p.Amir).ThenBy(p => p.AdSoyad).ToList();
                return Result<List<BolgePersoneli>>.Success(liste);
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.GetBolgePersoneliAsync hata.", ex);
                return Result<List<BolgePersoneli>>.Failure(Error.Unexpected("Bölge personeli alınamadı.", ex));
            }
        }

        public async Task<Result<List<TasinmazKontrolOzet>>> GetListeAsync(int? tasinmazId = null, KontrolDurumu? durum = null, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var tasinmazlar = BolgeyeGoreFiltrele(db, await YetkiliBolgelerAsync(cancellationToken));

                var q = from k in db.TasinmazKontrol_Table.AsNoTracking()
                        join t in tasinmazlar on k.TasinmazId equals t.Id
                        select new TasinmazKontrolOzet
                        {
                            Id = k.Id,
                            TasinmazId = t.Id,
                            EmlakSicilNo = t.EmlakSicilNo,
                            Cinsi = t.Cinsi,
                            Ili = t.Ili,
                            Ilcesi = t.Ilcesi,
                            BolgeAdi = k.BolgeAdi,
                            KontrolTarihi = k.KontrolTarihi,
                            Durum = k.Durum,
                            KontrolEdenler = k.KontrolEdenler,
                            RaporNo = k.RaporNo,
                            SonrakiKontrolTarihi = k.ElleBelirlenenSonrakiTarih ?? k.HesaplananSonrakiTarih,
                            RevizeEdilenKontrolId = k.RevizeEdilenKontrolId
                        };

                if (tasinmazId.HasValue) q = q.Where(x => x.TasinmazId == tasinmazId.Value);
                if (durum.HasValue) q = q.Where(x => x.Durum == durum.Value);

                var list = await q.OrderByDescending(x => x.KontrolTarihi).ThenByDescending(x => x.Id).ToListAsync(cancellationToken);
                return Result<List<TasinmazKontrolOzet>>.Success(list);
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.GetListeAsync hata.", ex);
                return Result<List<TasinmazKontrolOzet>>.Failure(Error.Unexpected("Kontrol listesi alınamadı.", ex));
            }
        }

        public async Task<Result<TasinmazKontrol>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var k = await db.TasinmazKontrol_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                if (k is null) return Result<TasinmazKontrol>.Failure(Error.NotFound("Kontrol bulunamadı."));
                if (!await KontroleErisebilirAsync(db, k.TasinmazId, cancellationToken))
                    return Result<TasinmazKontrol>.Failure(Error.Forbidden(BolgeYetkisiYok));
                return Result<TasinmazKontrol>.Success(k);
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.GetByIdAsync hata.", ex);
                return Result<TasinmazKontrol>.Failure(Error.Unexpected("Kontrol alınamadı.", ex));
            }
        }

        public async Task<Result<List<TasinmazKontrolEk>>> GetEklerAsync(int kontrolId, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var tasinmazId = await db.TasinmazKontrol_Table.Where(k => k.Id == kontrolId).Select(k => (int?)k.TasinmazId).FirstOrDefaultAsync(cancellationToken);
                if (tasinmazId is null) return Result<List<TasinmazKontrolEk>>.Failure(Error.NotFound("Kontrol bulunamadı."));
                if (!await KontroleErisebilirAsync(db, tasinmazId.Value, cancellationToken))
                    return Result<List<TasinmazKontrolEk>>.Failure(Error.Forbidden(BolgeYetkisiYok));

                var list = await db.TasinmazKontrolEk_Table.AsNoTracking().Where(e => e.KontrolId == kontrolId).OrderBy(e => e.Id).ToListAsync(cancellationToken);
                return Result<List<TasinmazKontrolEk>>.Success(list);
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.GetEklerAsync hata.", ex);
                return Result<List<TasinmazKontrolEk>>.Failure(Error.Unexpected("Ekler alınamadı.", ex));
            }
        }

        public async Task<Result<TasinmazKontrolEk>> GetEkAsync(int ekId, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var ek = await db.TasinmazKontrolEk_Table.AsNoTracking().FirstOrDefaultAsync(e => e.Id == ekId, cancellationToken);
                if (ek is null) return Result<TasinmazKontrolEk>.Failure(Error.NotFound("Ek bulunamadı."));
                var tasinmazId = await db.TasinmazKontrol_Table.Where(k => k.Id == ek.KontrolId).Select(k => k.TasinmazId).FirstAsync(cancellationToken);
                if (!await KontroleErisebilirAsync(db, tasinmazId, cancellationToken))
                    return Result<TasinmazKontrolEk>.Failure(Error.Forbidden(BolgeYetkisiYok));
                return Result<TasinmazKontrolEk>.Success(ek);
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.GetEkAsync hata.", ex);
                return Result<TasinmazKontrolEk>.Failure(Error.Unexpected("Ek alınamadı.", ex));
            }
        }

        public async Task<Result<List<TasinmazPlanOzet>>> GetPlanListesiAsync(bool sadeceGecikmis = false, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var tasinmazlar = BolgeyeGoreFiltrele(db, await YetkiliBolgelerAsync(cancellationToken))
                    .Where(t => t.EnvanterdeMi == 1);
                var bugun = DateTime.Today;

                var q = from t in tasinmazlar
                        join p0 in db.TasinmazKontrolPlani_Table.AsNoTracking() on t.Id equals p0.TasinmazId into pj
                        from p in pj.DefaultIfEmpty()
                        select new TasinmazPlanOzet
                        {
                            TasinmazId = t.Id,
                            EmlakSicilNo = t.EmlakSicilNo,
                            Adres = t.Adres,
                            AdaNo = t.AdaNo,
                            ParselNo = t.ParselNo,
                            Cinsi = t.Cinsi,
                            Ili = t.Ili,
                            Ilcesi = t.Ilcesi,
                            BolgeAdi = db.Il_Table.Where(il => il.Id == t.IlId)
                                .Select(il => db.Bolge_Table.Where(b => b.Id == il.BolgeId).Select(b => b.Adi).FirstOrDefault())
                                .FirstOrDefault(),
                            PeriyotSure = p != null ? p.PeriyotSure : null,
                            PeriyotBirim = p != null ? p.PeriyotBirim : null,
                            SonrakiKontrolTarihi = p != null ? p.SonrakiKontrolTarihi : null,
                            SonKontrolTarihi = db.TasinmazKontrol_Table
                                .Where(k => k.TasinmazId == t.Id && k.Durum == KontrolDurumu.Tamamlandi)
                                .OrderByDescending(k => k.KontrolTarihi).Select(k => (DateTime?)k.KontrolTarihi).FirstOrDefault(),
                            SonKontrolId = db.TasinmazKontrol_Table
                                .Where(k => k.TasinmazId == t.Id && k.Durum == KontrolDurumu.Tamamlandi)
                                .OrderByDescending(k => k.KontrolTarihi).Select(k => (int?)k.Id).FirstOrDefault(),
                            AcikTaslakId = db.TasinmazKontrol_Table
                                .Where(k => k.TasinmazId == t.Id && k.Durum == KontrolDurumu.Taslak)
                                .OrderByDescending(k => k.Id).Select(k => (int?)k.Id).FirstOrDefault()
                        };

                if (sadeceGecikmis) q = q.Where(x => x.SonrakiKontrolTarihi != null && x.SonrakiKontrolTarihi < bugun);

                var list = await q.OrderBy(x => x.AcikTaslakId == null).ThenBy(x => x.SonrakiKontrolTarihi == null).ThenBy(x => x.SonrakiKontrolTarihi).ThenBy(x => x.EmlakSicilNo).ToListAsync(cancellationToken);
                return Result<List<TasinmazPlanOzet>>.Success(list);
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.GetPlanListesiAsync hata.", ex);
                return Result<List<TasinmazPlanOzet>>.Failure(Error.Unexpected("Plan listesi alınamadı.", ex));
            }
        }

        public async Task<Result<TasinmazKontrolPlani?>> GetPlanAsync(int tasinmazId, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                if (!await KontroleErisebilirAsync(db, tasinmazId, cancellationToken))
                    return Result<TasinmazKontrolPlani?>.Failure(Error.Forbidden(BolgeYetkisiYok));
                var p = await db.TasinmazKontrolPlani_Table.AsNoTracking().FirstOrDefaultAsync(x => x.TasinmazId == tasinmazId, cancellationToken);
                return Result<TasinmazKontrolPlani?>.Success(p);
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.GetPlanAsync hata.", ex);
                return Result<TasinmazKontrolPlani?>.Failure(Error.Unexpected("Plan alınamadı.", ex));
            }
        }

        // ----------------------------------------------------------------- Komutlar

        public async Task<Result<int>> BaslatAsync(KontrolBaslatIstegi istek, CancellationToken cancellationToken = default)
        {
            var yetki = await CheckAsync(ModuleName.TBYS, Operation.Create, cancellationToken);
            if (yetki is not null) return Result<int>.Failure(yetki.Error);
            if (istek is null) return Result<int>.Failure(Error.Validation("İstek boş olamaz."));
            if (istek.KontrolTarihi.Date > DateTime.Today) return Result<int>.Failure(Error.Validation("Kontrol tarihi ileri tarihli olamaz."));
            if (istek.RevizeEdilenKontrolId.HasValue && string.IsNullOrWhiteSpace(istek.RevizyonGerekcesi))
                return Result<int>.Failure(Error.Validation("Revizyon için gerekçe zorunludur."));

            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var yt = await YetkiliTasinmazAsync(db, istek.TasinmazId, cancellationToken);
                if (yt is null) return Result<int>.Failure(Error.Forbidden(BolgeYetkisiYok));
                var (tasinmaz, bolgeId, bolgeAdi) = yt.Value;

                if (await db.TasinmazKontrol_Table.AnyAsync(k => k.TasinmazId == istek.TasinmazId && k.Durum == KontrolDurumu.Taslak, cancellationToken))
                    return Result<int>.Failure(Error.Conflict("Bu taşınmaz için açık bir taslak kontrol zaten var."));

                TasinmazKontrol? revizeEdilen = null;
                if (istek.RevizeEdilenKontrolId.HasValue)
                {
                    revizeEdilen = await db.TasinmazKontrol_Table.AsNoTracking().FirstOrDefaultAsync(k => k.Id == istek.RevizeEdilenKontrolId.Value, cancellationToken);
                    if (revizeEdilen is null || revizeEdilen.TasinmazId != istek.TasinmazId) return Result<int>.Failure(Error.NotFound("Revize edilecek kontrol bulunamadı."));
                    if (revizeEdilen.Durum != KontrolDurumu.Tamamlandi) return Result<int>.Failure(Error.Validation("Yalnızca tamamlanmış kontroller revize edilebilir."));
                    if (await db.TasinmazKontrol_Table.AnyAsync(k => k.RevizeEdilenKontrolId == revizeEdilen.Id, cancellationToken))
                        return Result<int>.Failure(Error.Conflict("Bu kontrol için zaten bir revizyon açılmış."));
                }

                var tanim = KontrolRaporu.TanimAl(revizeEdilen?.TasinmazSnapshotJson);
                var snapshot = TasinmazVeriKaynagi.Snapshot(tasinmaz, tanim);
                KontrolRaporu.GecmisBilgileriniKoru(snapshot, revizeEdilen?.TasinmazSnapshotJson);

                var cevaplar = revizeEdilen is not null
                    ? KontrolCevaplari.FromJson(revizeEdilen.CevaplarJson)
                    : new KontrolCevaplari();
                foreach (var alan in tanim.TumAlanlar().Where(a => a.VeriKaynagi != KontrolVeriKaynagiTipi.Manuel && a.TasinmazAlani != null))
                {
                    var saltOkunur = alan.VeriKaynagi == KontrolVeriKaynagiTipi.TasinmazSaltOkunur;
                    if (saltOkunur || revizeEdilen is null || string.IsNullOrWhiteSpace(cevaplar.Al(alan.Kod)))
                        cevaplar.Alanlar[alan.Kod] = snapshot.GetValueOrDefault(alan.TasinmazAlani!);
                }

                var kullanici = await GetUserNameAsync(cancellationToken);
                var kontrol = new TasinmazKontrol
                {
                    TasinmazId = istek.TasinmazId,
                    BolgeId = bolgeId,
                    BolgeAdi = bolgeAdi,
                    KontrolEdenler = istek.KontrolEdenler?.Trim(),
                    ImzalayanMakam = istek.ImzalayanMakam?.Trim(),
                    KontrolTarihi = istek.KontrolTarihi.Date,
                    Durum = KontrolDurumu.Taslak,
                    TasinmazSnapshotJson = System.Text.Json.JsonSerializer.Serialize(snapshot, KontrolFormTanimi.JsonOptions),
                    CevaplarJson = cevaplar.ToJson(),
                    RevizeEdilenKontrolId = revizeEdilen?.Id,
                    IslemGerekcesi = revizeEdilen is null ? null : istek.RevizyonGerekcesi!.Trim(),
                    IslemYapan = revizeEdilen is null ? null : kullanici,
                    IslemTarihi = revizeEdilen is null ? null : DateTime.Now
                };

                db.SetCurrentUser(kullanici);
                db.TasinmazKontrol_Table.Add(kontrol);
                await db.SaveChangesAsync(cancellationToken);
                return Result<int>.Success(kontrol.Id);
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.BaslatAsync hata.", ex);
                return Result<int>.Failure(Error.Unexpected("Kontrol başlatılamadı.", ex));
            }
        }

        public async Task<Result> TaslakKaydetAsync(int kontrolId, int kayitSurumu, KontrolCevaplari cevaplar, string? kontrolEdenler, DateTime kontrolTarihi, string? imzalayanMakam = null, CancellationToken cancellationToken = default)
        {
            var yetki = await CheckAsync(ModuleName.TBYS, Operation.Edit, cancellationToken);
            if (yetki is not null) return yetki;
            if (cevaplar is null) return Result.Failure(Error.Validation("Cevaplar boş olamaz."));
            if (kontrolTarihi.Date > DateTime.Today) return Result.Failure(Error.Validation("Kontrol tarihi ileri tarihli olamaz."));

            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var k = await db.TasinmazKontrol_Table.FirstOrDefaultAsync(x => x.Id == kontrolId, cancellationToken);
                if (k is null) return Result.Failure(Error.NotFound("Kontrol bulunamadı."));
                if (!await KontroleErisebilirAsync(db, k.TasinmazId, cancellationToken)) return Result.Failure(Error.Forbidden(BolgeYetkisiYok));
                if (k.Durum != KontrolDurumu.Taslak) return Result.Failure(Error.Conflict("Yalnızca taslak kontroller düzenlenebilir."));
                if (k.KayitSurumu != kayitSurumu) return Result.Failure(Error.Conflict(EszamanlilikMesaji));

                var tanim = KontrolRaporu.TanimAl(k.TasinmazSnapshotJson);
                var ekler = await EkSayilariAsync(db, kontrolId, cancellationToken);
                var hatalar = KontrolFormDogrulayici.CevaplariDogrula(tanim, cevaplar, (kod, satir) => ekler.GetValueOrDefault((kod, satir)), zorunluKontrol: false);
                if (hatalar.Count > 0) return Result.Failure(Error.Validation(string.Join(" ", hatalar)));

                SaltOkunurlariKoru(tanim, cevaplar, KontrolCevaplari.FromJson(k.CevaplarJson));

                k.CevaplarJson = cevaplar.ToJson();
                k.KontrolEdenler = kontrolEdenler?.Trim();
                k.ImzalayanMakam = imzalayanMakam?.Trim();
                k.KontrolTarihi = kontrolTarihi.Date;
                k.KayitSurumu++;
                db.SetCurrentUser(await GetUserNameAsync(cancellationToken));
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (DbUpdateConcurrencyException) { return Result.Failure(Error.Conflict(EszamanlilikMesaji)); }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.TaslakKaydetAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Taslak kaydedilemedi.", ex));
            }
        }

        public async Task<Result> TamamlaAsync(KontrolTamamlaIstegi istek, CancellationToken cancellationToken = default)
        {
            var yetki = await CheckAsync(ModuleName.TBYS, Operation.Edit, cancellationToken);
            if (yetki is not null) return yetki;
            if (istek is null) return Result.Failure(Error.Validation("İstek boş olamaz."));
            if (istek.KontrolTarihi.Date > DateTime.Today) return Result.Failure(Error.Validation("Kontrol tarihi ileri tarihli olamaz."));
            if (istek.PeriyotSure.HasValue != istek.PeriyotBirim.HasValue || istek.PeriyotSure is <= 0)
                return Result.Failure(Error.Validation("Periyot için süre ve birim birlikte, pozitif olarak girilmelidir."));
            if (istek.ElleBelirlenenSonrakiTarih.HasValue)
            {
                if (string.IsNullOrWhiteSpace(istek.TarihDegisiklikGerekcesi)) return Result.Failure(Error.Validation("Elle belirlenen tarih için gerekçe zorunludur."));
                if (istek.ElleBelirlenenSonrakiTarih.Value.Date <= istek.KontrolTarihi.Date) return Result.Failure(Error.Validation("Sonraki kontrol tarihi, kontrol tarihinden sonra olmalıdır."));
            }

            try
            {
                await using var scope = await _scopeFactory.CreateAsync(cancellationToken);
                var db = scope.Context;
                var kullanici = await GetUserNameAsync(cancellationToken);
                db.SetCurrentUser(kullanici);

                var k = await db.TasinmazKontrol_Table.FirstOrDefaultAsync(x => x.Id == istek.KontrolId, cancellationToken);
                if (k is null) return Result.Failure(Error.NotFound("Kontrol bulunamadı."));
                if (!await KontroleErisebilirAsync(db, k.TasinmazId, cancellationToken)) return Result.Failure(Error.Forbidden(BolgeYetkisiYok));
                if (k.Durum != KontrolDurumu.Taslak) return Result.Failure(Error.Conflict("Yalnızca taslak kontroller tamamlanabilir."));
                if (k.KayitSurumu != istek.KayitSurumu) return Result.Failure(Error.Conflict(EszamanlilikMesaji));

                var tanim = KontrolRaporu.TanimAl(k.TasinmazSnapshotJson);
                var ekler = await EkSayilariAsync(db, k.Id, cancellationToken);
                var hatalar = KontrolFormDogrulayici.CevaplariDogrula(tanim, istek.Cevaplar, (kod, satir) => ekler.GetValueOrDefault((kod, satir)), zorunluKontrol: true);
                if (string.IsNullOrWhiteSpace(istek.ImzalayanMakam)) hatalar.Insert(0, "'Raporu İmzalayan Makam' zorunludur.");
                if (hatalar.Count > 0) return Result.Failure(Error.Validation(string.Join(" ", hatalar)));
                SaltOkunurlariKoru(tanim, istek.Cevaplar, KontrolCevaplari.FromJson(k.CevaplarJson));

                var plan = await db.TasinmazKontrolPlani_Table.FirstOrDefaultAsync(p => p.TasinmazId == k.TasinmazId, cancellationToken);
                var varsayilanPeriyot = KontrolRaporu.PeriyotAl(k.TasinmazSnapshotJson);
                var oncekiSure = plan?.PeriyotSure ?? varsayilanPeriyot.Sure;
                var oncekiBirim = plan?.PeriyotBirim ?? varsayilanPeriyot.Birim;

                var secilenSure = istek.PeriyotSure ?? oncekiSure;
                var secilenBirim = istek.PeriyotBirim ?? oncekiBirim;
                var periyotDegisti = plan is not null && plan.PeriyotTanimli && !KontrolPeriyodu.Ayni(plan.PeriyotSure, plan.PeriyotBirim, secilenSure, secilenBirim);
                if (periyotDegisti && string.IsNullOrWhiteSpace(istek.PeriyotDegisiklikGerekcesi))
                    return Result.Failure(Error.Validation("Periyot değişikliği için gerekçe zorunludur."));

                var simdi = DateTime.Now;
                k.Durum = KontrolDurumu.Tamamlandi;
                k.TamamlanmaTarihi = simdi;
                k.Tamamlayan = kullanici;
                k.KontrolTarihi = istek.KontrolTarihi.Date;
                k.KontrolEdenler = istek.KontrolEdenler?.Trim();
                k.ImzalayanMakam = istek.ImzalayanMakam!.Trim();
                k.RaporNo ??= await RaporNoUretAsync(db, k, cancellationToken);
                k.CevaplarJson = istek.Cevaplar.ToJson();
                k.OncekiPeriyotSure = oncekiSure;
                k.OncekiPeriyotBirim = oncekiBirim;
                k.SecilenPeriyotSure = secilenSure;
                k.SecilenPeriyotBirim = secilenBirim;
                k.PeriyotDegisiklikGerekcesi = periyotDegisti ? istek.PeriyotDegisiklikGerekcesi!.Trim() : null;
                k.HesaplananSonrakiTarih = KontrolPeriyodu.Gecerli(secilenSure, secilenBirim)
                    ? KontrolPeriyodu.Ekle(k.KontrolTarihi, secilenSure!.Value, secilenBirim!.Value)
                    : null;
                k.ElleBelirlenenSonrakiTarih = istek.ElleBelirlenenSonrakiTarih?.Date;
                k.TarihDegisiklikGerekcesi = istek.ElleBelirlenenSonrakiTarih.HasValue ? istek.TarihDegisiklikGerekcesi!.Trim() : null;
                k.KayitSurumu++;

                // Plan yalnızca daha yeni/en yeni kontrolse güncellenir; geçmiş tarihli kontrol planı geri almaz.
                var dahaYeniVar = await db.TasinmazKontrol_Table.AnyAsync(x =>
                    x.TasinmazId == k.TasinmazId && x.Id != k.Id && x.Durum == KontrolDurumu.Tamamlandi && x.KontrolTarihi > k.KontrolTarihi, cancellationToken);

                if (!dahaYeniVar)
                {
                    if (plan is null)
                    {
                        plan = new TasinmazKontrolPlani { TasinmazId = k.TasinmazId };
                        db.TasinmazKontrolPlani_Table.Add(plan);
                    }
                    plan.PeriyotSure = secilenSure;
                    plan.PeriyotBirim = secilenBirim;
                    plan.SonrakiKontrolTarihi = k.SonrakiKontrolTarihi;
                    plan.KaynakKontrolId = k.Id;
                    plan.DegisiklikGerekcesi = k.PeriyotDegisiklikGerekcesi ?? k.TarihDegisiklikGerekcesi;
                    plan.KayitSurumu++;
                    k.PlanGuncellendi = true;
                }

                // Revizyon tamamlanınca orijinal kayıt "RevizeEdildi" olur (içeriği korunur).
                if (k.RevizeEdilenKontrolId.HasValue)
                {
                    var orijinal = await db.TasinmazKontrol_Table.FirstOrDefaultAsync(x => x.Id == k.RevizeEdilenKontrolId.Value, cancellationToken);
                    if (orijinal is not null && orijinal.Durum == KontrolDurumu.Tamamlandi)
                    {
                        orijinal.Durum = KontrolDurumu.RevizeEdildi;
                        orijinal.IslemGerekcesi = k.IslemGerekcesi;
                        orijinal.IslemYapan = kullanici;
                        orijinal.IslemTarihi = simdi;
                        orijinal.KayitSurumu++;
                    }
                }

                await scope.CommitAsync(cancellationToken);
                return Result.Success();
            }
            catch (DbUpdateConcurrencyException) { return Result.Failure(Error.Conflict(EszamanlilikMesaji)); }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex)) { return Result.Failure(Error.Conflict(EszamanlilikMesaji)); }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.TamamlaAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Kontrol tamamlanamadı.", ex));
            }
        }

        public async Task<Result> IptalEtAsync(int kontrolId, string gerekce, CancellationToken cancellationToken = default)
        {
            var yetki = await CheckAsync(ModuleName.TBYS, Operation.Edit, cancellationToken);
            if (yetki is not null) return yetki;
            if (string.IsNullOrWhiteSpace(gerekce)) return Result.Failure(Error.Validation("İptal gerekçesi zorunludur."));

            try
            {
                await using var scope = await _scopeFactory.CreateAsync(cancellationToken);
                var db = scope.Context;
                var kullanici = await GetUserNameAsync(cancellationToken);
                db.SetCurrentUser(kullanici);

                var k = await db.TasinmazKontrol_Table.FirstOrDefaultAsync(x => x.Id == kontrolId, cancellationToken);
                if (k is null) return Result.Failure(Error.NotFound("Kontrol bulunamadı."));
                if (!await KontroleErisebilirAsync(db, k.TasinmazId, cancellationToken)) return Result.Failure(Error.Forbidden(BolgeYetkisiYok));
                if (k.Durum is KontrolDurumu.IptalEdildi or KontrolDurumu.RevizeEdildi) return Result.Failure(Error.Conflict("Kontrol zaten kapatılmış."));

                k.Durum = KontrolDurumu.IptalEdildi;
                k.IslemGerekcesi = gerekce.Trim();
                k.IslemYapan = kullanici;
                k.IslemTarihi = DateTime.Now;
                k.KayitSurumu++;

                // Plan bu kontrolden türemişse bir önceki tamamlanmış kontrole geri döndür.
                var plan = await db.TasinmazKontrolPlani_Table.FirstOrDefaultAsync(p => p.TasinmazId == k.TasinmazId, cancellationToken);
                if (plan is not null && plan.KaynakKontrolId == k.Id)
                {
                    var onceki = await db.TasinmazKontrol_Table.AsNoTracking()
                        .Where(x => x.TasinmazId == k.TasinmazId && x.Id != k.Id && x.Durum == KontrolDurumu.Tamamlandi)
                        .OrderByDescending(x => x.KontrolTarihi).ThenByDescending(x => x.Id)
                        .FirstOrDefaultAsync(cancellationToken);
                    plan.PeriyotSure = onceki?.SecilenPeriyotSure;
                    plan.PeriyotBirim = onceki?.SecilenPeriyotBirim;
                    plan.SonrakiKontrolTarihi = onceki?.SonrakiKontrolTarihi;
                    plan.KaynakKontrolId = onceki?.Id;
                    plan.DegisiklikGerekcesi = $"Kontrol #{k.Id} iptal edildi: {k.IslemGerekcesi}";
                    plan.KayitSurumu++;
                }

                await scope.CommitAsync(cancellationToken);
                return Result.Success();
            }
            catch (DbUpdateConcurrencyException) { return Result.Failure(Error.Conflict(EszamanlilikMesaji)); }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.IptalEtAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Kontrol iptal edilemedi.", ex));
            }
        }

        public async Task<Result> DeleteAsync(int kontrolId, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var scope = await _scopeFactory.CreateAsync(cancellationToken);
                var db = scope.Context;
                var k = await db.TasinmazKontrol_Table.FirstOrDefaultAsync(x => x.Id == kontrolId, cancellationToken);
                if (k is null) return Result.Failure(Error.NotFound("Kontrol bulunamadı."));
                if (!await KontroleErisebilirAsync(db, k.TasinmazId, cancellationToken)) return Result.Failure(Error.Forbidden(BolgeYetkisiYok));
                if (k.Durum != KontrolDurumu.Taslak) return Result.Failure(Error.Conflict("Yalnızca taslak kontroller silinebilir; tamamlanmış kontroller iptal edilir."));

                var ekler = await db.TasinmazKontrolEk_Table.Where(e => e.KontrolId == kontrolId).ToListAsync(cancellationToken);
                db.TasinmazKontrolEk_Table.RemoveRange(ekler);
                db.TasinmazKontrol_Table.Remove(k);
                await scope.CommitAsync(cancellationToken);

                foreach (var e in ekler)
                {
                    try { _ekDeposu.Sil(e.SaklananAd); }
                    catch (Exception ex) { _logService.LogError($"{Source}.DeleteAsync ek dosyası silinemedi: {e.SaklananAd}", ex); }
                }
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.DeleteAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Kontrol silinemedi.", ex));
            }
        }

        public async Task<Result> PlanGuncelleAsync(PlanGuncelleIstegi istek, CancellationToken cancellationToken = default)
        {
            var yetki = await CheckAsync(ModuleName.TBYS, Operation.Edit, cancellationToken);
            if (yetki is not null) return yetki;
            if (istek is null) return Result.Failure(Error.Validation("İstek boş olamaz."));
            if (string.IsNullOrWhiteSpace(istek.Gerekce)) return Result.Failure(Error.Validation("Plan değişikliği için gerekçe zorunludur."));
            if (istek.PeriyotSure.HasValue != istek.PeriyotBirim.HasValue || istek.PeriyotSure is <= 0)
                return Result.Failure(Error.Validation("Periyot için süre ve birim birlikte, pozitif olarak girilmelidir."));
            if (istek.SonrakiKontrolTarihi is null && istek.PeriyotSure is null)
                return Result.Failure(Error.Validation("Periyot veya sonraki kontrol tarihinden en az biri girilmelidir."));

            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                if (!await KontroleErisebilirAsync(db, istek.TasinmazId, cancellationToken)) return Result.Failure(Error.Forbidden(BolgeYetkisiYok));

                var plan = await db.TasinmazKontrolPlani_Table.FirstOrDefaultAsync(p => p.TasinmazId == istek.TasinmazId, cancellationToken);
                if (plan is null)
                {
                    plan = new TasinmazKontrolPlani { TasinmazId = istek.TasinmazId };
                    db.TasinmazKontrolPlani_Table.Add(plan);
                }

                if (istek.PeriyotSure.HasValue)
                {
                    plan.PeriyotSure = istek.PeriyotSure;
                    plan.PeriyotBirim = istek.PeriyotBirim;
                }
                if (istek.SonrakiKontrolTarihi.HasValue)
                    plan.SonrakiKontrolTarihi = istek.SonrakiKontrolTarihi.Value.Date;
                else if (plan.PeriyotTanimli)
                {
                    var sonKontrol = await db.TasinmazKontrol_Table.AsNoTracking()
                        .Where(k => k.TasinmazId == istek.TasinmazId && k.Durum == KontrolDurumu.Tamamlandi)
                        .OrderByDescending(k => k.KontrolTarihi).Select(k => (DateTime?)k.KontrolTarihi).FirstOrDefaultAsync(cancellationToken);
                    plan.SonrakiKontrolTarihi = KontrolPeriyodu.Ekle(sonKontrol ?? DateTime.Today, plan.PeriyotSure!.Value, plan.PeriyotBirim!.Value);
                }

                plan.KaynakKontrolId = null;
                plan.DegisiklikGerekcesi = istek.Gerekce.Trim();
                plan.KayitSurumu++;

                db.SetCurrentUser(await GetUserNameAsync(cancellationToken));
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (DbUpdateConcurrencyException) { return Result.Failure(Error.Conflict(EszamanlilikMesaji)); }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex)) { return Result.Failure(Error.Conflict(EszamanlilikMesaji)); }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.PlanGuncelleAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Plan güncellenemedi.", ex));
            }
        }

        // ----------------------------------------------------------------- Ekler

        public async Task<Result<int>> EkYukleAsync(EkYukleIstegi istek, CancellationToken cancellationToken = default)
        {
            var yetki = await CheckAsync(ModuleName.TBYS, Operation.Edit, cancellationToken);
            if (yetki is not null) return Result<int>.Failure(yetki.Error);
            if (istek is null || string.IsNullOrWhiteSpace(istek.DosyaAdi) || string.IsNullOrWhiteSpace(istek.AlanKodu))
                return Result<int>.Failure(Error.Validation("Dosya ve alan bilgisi zorunludur."));

            var uzanti = Path.GetExtension(istek.DosyaAdi);
            if (!IzinliUzantilar.Contains(uzanti))
                return Result<int>.Failure(Error.Validation($"İzin verilmeyen dosya türü: {uzanti}"));

            string? saklananAd = null;
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var k = await db.TasinmazKontrol_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == istek.KontrolId, cancellationToken);
                if (k is null) return Result<int>.Failure(Error.NotFound("Kontrol bulunamadı."));
                if (!await KontroleErisebilirAsync(db, k.TasinmazId, cancellationToken)) return Result<int>.Failure(Error.Forbidden(BolgeYetkisiYok));
                if (k.Durum != KontrolDurumu.Taslak) return Result<int>.Failure(Error.Conflict("Yalnızca taslak kontrollere ek yüklenebilir."));

                var tanim = KontrolRaporu.TanimAl(k.TasinmazSnapshotJson);
                var alan = tanim.AlanBul(istek.AlanKodu);
                if (alan is null || alan.Tip != KontrolAlanTipi.Ek) return Result<int>.Failure(Error.Validation("Alan bir ek alanı değil."));

                saklananAd = $"{Guid.NewGuid():N}{uzanti.ToLowerInvariant()}";
                var boyut = await _ekDeposu.KaydetAsync(saklananAd, istek.Icerik, cancellationToken);
                if (boyut > DosyaSistemiKontrolEkDeposu.MaksimumBoyut)
                {
                    _ekDeposu.Sil(saklananAd);
                    return Result<int>.Failure(Error.Validation("Dosya 20 MB sınırını aşıyor."));
                }

                var ek = new TasinmazKontrolEk
                {
                    KontrolId = k.Id,
                    AlanKodu = alan.Kod,
                    GrupKodu = istek.GrupKodu,
                    SatirNo = istek.SatirNo,
                    DosyaAdi = Path.GetFileName(istek.DosyaAdi),
                    SaklananAd = saklananAd,
                    IcerikTipi = istek.IcerikTipi,
                    Boyut = boyut
                };
                db.SetCurrentUser(await GetUserNameAsync(cancellationToken));
                db.TasinmazKontrolEk_Table.Add(ek);
                await db.SaveChangesAsync(cancellationToken);
                return Result<int>.Success(ek.Id);
            }
            catch (Exception ex)
            {
                if (saklananAd is not null) { try { _ekDeposu.Sil(saklananAd); } catch { /* en iyi çaba */ } }
                _logService.LogError($"{Source}.EkYukleAsync hata.", ex);
                return Result<int>.Failure(Error.Unexpected("Ek yüklenemedi.", ex));
            }
        }

        public async Task<Result> EkSilAsync(int ekId, CancellationToken cancellationToken = default)
        {
            var yetki = await CheckAsync(ModuleName.TBYS, Operation.Edit, cancellationToken);
            if (yetki is not null) return yetki;

            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var ek = await db.TasinmazKontrolEk_Table.FirstOrDefaultAsync(e => e.Id == ekId, cancellationToken);
                if (ek is null) return Result.Failure(Error.NotFound("Ek bulunamadı."));
                var k = await db.TasinmazKontrol_Table.AsNoTracking().FirstAsync(x => x.Id == ek.KontrolId, cancellationToken);
                if (!await KontroleErisebilirAsync(db, k.TasinmazId, cancellationToken)) return Result.Failure(Error.Forbidden(BolgeYetkisiYok));
                if (k.Durum != KontrolDurumu.Taslak) return Result.Failure(Error.Conflict("Tamamlanmış kontrolün ekleri silinemez."));

                db.TasinmazKontrolEk_Table.Remove(ek);
                await db.SaveChangesAsync(cancellationToken);
                try { _ekDeposu.Sil(ek.SaklananAd); }
                catch (Exception ex) { _logService.LogError($"{Source}.EkSilAsync dosya silinemedi: {ek.SaklananAd}", ex); }
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.EkSilAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Ek silinemedi.", ex));
            }
        }

        public async Task<Result<(Stream Icerik, TasinmazKontrolEk Ek)>> EkAcAsync(int ekId, CancellationToken cancellationToken = default)
        {
            var yetki = await CheckAsync(ModuleName.TBYS, Operation.View, cancellationToken);
            if (yetki is not null) return Result<(Stream, TasinmazKontrolEk)>.Failure(yetki.Error);

            var ekResult = await GetEkAsync(ekId, cancellationToken);
            if (ekResult.IsFailure) return Result<(Stream, TasinmazKontrolEk)>.Failure(ekResult.Error);

            try
            {
                var stream = _ekDeposu.Ac(ekResult.Value.SaklananAd);
                return stream is null
                    ? Result<(Stream, TasinmazKontrolEk)>.Failure(Error.NotFound("Ek dosyası depoda bulunamadı."))
                    : Result<(Stream, TasinmazKontrolEk)>.Success((stream, ekResult.Value));
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.EkAcAsync hata.", ex);
                return Result<(Stream, TasinmazKontrolEk)>.Failure(Error.Unexpected("Ek açılamadı.", ex));
            }
        }

        // ----------------------------------------------------------------- Yardımcılar

        private static async Task<Dictionary<(string, int?), int>> EkSayilariAsync(ApplicationDbContext db, int kontrolId, CancellationToken ct)
        {
            var ekler = await db.TasinmazKontrolEk_Table.AsNoTracking()
                .Where(e => e.KontrolId == kontrolId)
                .Select(e => new { e.AlanKodu, e.SatirNo })
                .ToListAsync(ct);
            return ekler.GroupBy(e => (e.AlanKodu, e.SatirNo)).ToDictionary(g => g.Key, g => g.Count());
        }

        /// <summary>Salt okunur taşınmaz alanları istemciden gelen değerle değil, kayıtlı snapshot değeriyle kalır.</summary>
        private static void SaltOkunurlariKoru(KontrolFormTanimi tanim, KontrolCevaplari yeni, KontrolCevaplari mevcut)
        {
            foreach (var alan in tanim.TumAlanlar().Where(a => a.VeriKaynagi == KontrolVeriKaynagiTipi.TasinmazSaltOkunur))
            {
                if (mevcut.Alanlar.TryGetValue(alan.Kod, out var v)) yeni.Alanlar[alan.Kod] = v;
                else yeni.Alanlar.Remove(alan.Kod);
            }
        }

        private static bool IsUniqueViolation(DbUpdateException ex)
            => ex.InnerException is Microsoft.Data.SqlClient.SqlException sql && (sql.Number == 2601 || sql.Number == 2627);

        /// <summary>Rapor no: {BölgeKısaAdı}-{Plaka}-{Yıl}-{Sıra}. Sıra, bölge+yıl içinde tamamlanmış kontrol sayısına göre verilir.</summary>
        private static async Task<string> RaporNoUretAsync(ApplicationDbContext db, TasinmazKontrol k, CancellationToken ct)
        {
            var yil = k.KontrolTarihi.Year;
            var bolgeKisa = k.BolgeId is null ? null
                : await db.Bolge_Table.AsNoTracking().Where(b => b.Id == k.BolgeId).Select(b => b.KisaAdi ?? b.Adi).FirstOrDefaultAsync(ct);
            var plaka = await db.Tasinmaz_Table.AsNoTracking().Where(t => t.Id == k.TasinmazId)
                .Select(t => db.Il_Table.Where(il => il.Id == t.IlId).Select(il => il.PlakaKodu).FirstOrDefault()).FirstOrDefaultAsync(ct);

            var onek = $"{Kisalt(bolgeKisa)}-{(plaka is null ? "00" : plaka.Value.ToString("00"))}-{yil}-";
            var sonSira = await db.TasinmazKontrol_Table
                .Where(x => x.RaporNo != null && x.RaporNo.StartsWith(onek))
                .Select(x => x.RaporNo!.Substring(onek.Length))
                .ToListAsync(ct);
            var sira = sonSira.Select(s => int.TryParse(s, out var n) ? n : 0).DefaultIfEmpty(0).Max() + 1;
            return $"{onek}{sira:0000}";
        }

        private static string Kisalt(string? ad)
        {
            if (string.IsNullOrWhiteSpace(ad)) return "GM";
            var harfler = new string(ad.Trim().ToUpperInvariant().Where(char.IsLetterOrDigit).ToArray());
            return harfler.Length == 0 ? "GM" : harfler.Length <= 6 ? harfler : harfler[..6];
        }
    }
}
