using Jenga.DataAccess.Data;
using Jenga.DataAccess.Services.Common;
using Jenga.Models.Enums;
using Jenga.Models.TBYS.Kontrol;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.TBYS
{
    public class TasinmazKontrolFormService : ServiceBase, ITasinmazKontrolFormService
    {
        private const string Source = nameof(TasinmazKontrolFormService);

        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService _logService;

        public TasinmazKontrolFormService(
            IDbContextFactory<ApplicationDbContext> dbFactory,
            ILogService logService,
            IServiceAuthorizationContext? authContext = null) : base(authContext)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        }

        public async Task<Result<List<TasinmazKontrolForm>>> GetAllAsync(bool arsivDahil = false, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var q = db.TasinmazKontrolForm_Table.AsNoTracking();
                if (!arsivDahil) q = q.Where(f => f.Durum != KontrolFormDurumu.Arsivlenmis);
                var list = await q.OrderBy(f => f.Kod).ThenByDescending(f => f.Surum).ToListAsync(cancellationToken);
                return Result<List<TasinmazKontrolForm>>.Success(list);
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.GetAllAsync hata.", ex);
                return Result<List<TasinmazKontrolForm>>.Failure(Error.Unexpected("Form listesi alınamadı.", ex));
            }
        }

        public async Task<Result<TasinmazKontrolForm>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var f = await db.TasinmazKontrolForm_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                return f is null
                    ? Result<TasinmazKontrolForm>.Failure(Error.NotFound("Form bulunamadı."))
                    : Result<TasinmazKontrolForm>.Success(f);
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.GetByIdAsync hata.", ex);
                return Result<TasinmazKontrolForm>.Failure(Error.Unexpected("Form alınamadı.", ex));
            }
        }

        public async Task<Result<List<TasinmazKontrolForm>>> GetSurumlerAsync(string kod, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.TasinmazKontrolForm_Table.AsNoTracking()
                    .Where(f => f.Kod == kod)
                    .OrderByDescending(f => f.Surum)
                    .ToListAsync(cancellationToken);
                return Result<List<TasinmazKontrolForm>>.Success(list);
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.GetSurumlerAsync hata.", ex);
                return Result<List<TasinmazKontrolForm>>.Failure(Error.Unexpected("Form sürümleri alınamadı.", ex));
            }
        }

        public async Task<Result<List<TasinmazKontrolForm>>> GetUygunFormlarAsync(int tasinmazId, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var cins = await db.Tasinmaz_Table.AsNoTracking()
                    .Where(t => t.Id == tasinmazId)
                    .Select(t => t.Cinsi)
                    .FirstOrDefaultAsync(cancellationToken);
                var cinsNorm = TasinmazKontrolForm.CinsNormalize(cins);

                var list = await db.TasinmazKontrolForm_Table.AsNoTracking()
                    .Where(f => f.Durum == KontrolFormDurumu.Yayimlanmis &&
                                (f.Kapsam == KontrolFormKapsami.Genel ||
                                 (f.Kapsam == KontrolFormKapsami.Tasinmaz && f.KapsamTasinmazId == tasinmazId) ||
                                 (f.Kapsam == KontrolFormKapsami.Cins && cinsNorm != null && f.KapsamCinsi == cinsNorm)))
                    .ToListAsync(cancellationToken);

                list = list.OrderByDescending(f => (int)f.Kapsam).ThenBy(f => f.Ad).ToList();
                return Result<List<TasinmazKontrolForm>>.Success(list);
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.GetUygunFormlarAsync hata.", ex);
                return Result<List<TasinmazKontrolForm>>.Failure(Error.Unexpected("Uygun formlar alınamadı.", ex));
            }
        }

        public async Task<Result<int>> AddAsync(TasinmazKontrolForm form, CancellationToken cancellationToken = default)
        {
            var v = Dogrula(form);
            if (v is not null) return Result<int>.Failure(v);

            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                form.Kod = form.Kod.Trim().ToUpperInvariant();
                if (await db.TasinmazKontrolForm_Table.AnyAsync(f => f.Kod == form.Kod, cancellationToken))
                    return Result<int>.Failure(Error.Conflict("Bu kodla bir form zaten var."));

                form.Id = 0;
                form.Surum = 1;
                form.Durum = KontrolFormDurumu.Taslak;
                form.KaynakFormId = null;
                form.YayimTarihi = null; form.Yayimlayan = null; form.ArsivTarihi = null; form.Arsivleyen = null;
                form.KayitSurumu = 0;
                form.KapsamCinsi = form.Kapsam == KontrolFormKapsami.Cins ? TasinmazKontrolForm.CinsNormalize(form.KapsamCinsi) : null;
                form.KapsamTasinmazId = form.Kapsam == KontrolFormKapsami.Tasinmaz ? form.KapsamTasinmazId : null;

                db.TasinmazKontrolForm_Table.Add(form);
                await db.SaveChangesAsync(cancellationToken);
                return Result<int>.Success(form.Id);
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                return Result<int>.Failure(Error.Conflict("Bu kodla bir form zaten var."));
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.AddAsync hata.", ex);
                return Result<int>.Failure(Error.Unexpected("Form oluşturulamadı.", ex));
            }
        }

        public async Task<Result> UpdateAsync(TasinmazKontrolForm form, CancellationToken cancellationToken = default)
        {
            var v = Dogrula(form);
            if (v is not null) return Result.Failure(v);

            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var mevcut = await db.TasinmazKontrolForm_Table.FirstOrDefaultAsync(f => f.Id == form.Id, cancellationToken);
                if (mevcut is null) return Result.Failure(Error.NotFound("Form bulunamadı."));
                if (mevcut.Durum != KontrolFormDurumu.Taslak)
                    return Result.Failure(Error.Conflict("Yalnızca taslak sürümler düzenlenebilir. Değişiklik için yeni sürüm oluşturun."));
                if (mevcut.KayitSurumu != form.KayitSurumu)
                    return Result.Failure(Error.Conflict("Form başka bir kullanıcı tarafından değiştirildi. Sayfayı yenileyin."));

                mevcut.Ad = form.Ad.Trim();
                mevcut.Aciklama = form.Aciklama;
                mevcut.Kapsam = form.Kapsam;
                mevcut.KapsamCinsi = form.Kapsam == KontrolFormKapsami.Cins ? TasinmazKontrolForm.CinsNormalize(form.KapsamCinsi) : null;
                mevcut.KapsamTasinmazId = form.Kapsam == KontrolFormKapsami.Tasinmaz ? form.KapsamTasinmazId : null;
                mevcut.VarsayilanPeriyotSure = form.VarsayilanPeriyotSure;
                mevcut.VarsayilanPeriyotBirim = form.VarsayilanPeriyotBirim;
                mevcut.TanimJson = form.TanimJson;
                mevcut.KayitSurumu++;

                await db.SaveChangesAsync(cancellationToken);
                form.KayitSurumu = mevcut.KayitSurumu;
                return Result.Success();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Result.Failure(Error.Conflict("Form başka bir kullanıcı tarafından değiştirildi. Sayfayı yenileyin."));
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.UpdateAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Form güncellenemedi.", ex));
            }
        }

        public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var f = await db.TasinmazKontrolForm_Table.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                if (f is null) return Result.Failure(Error.NotFound("Form bulunamadı."));
                if (f.Durum != KontrolFormDurumu.Taslak)
                    return Result.Failure(Error.Conflict("Yayımlanmış veya arşivlenmiş sürümler silinemez; arşivleyin."));
                if (await db.TasinmazKontrol_Table.AnyAsync(k => k.FormId == id, cancellationToken))
                    return Result.Failure(Error.Conflict("Bu sürümle yapılmış kontrol kayıtları var; silinemez."));
                if (await db.TasinmazKontrolForm_Table.AnyAsync(x => x.KaynakFormId == id, cancellationToken))
                    return Result.Failure(Error.Conflict("Bu sürümden türetilmiş formlar var; silinemez."));

                db.TasinmazKontrolForm_Table.Remove(f);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.DeleteAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Form silinemedi.", ex));
            }
        }

        public async Task<Result<int>> YeniSurumOlusturAsync(int kaynakFormId, CancellationToken cancellationToken = default)
        {
            var yetki = await CheckAsync(ModuleName.TBYS, Operation.Create, cancellationToken);
            if (yetki is not null) return Result<int>.Failure(yetki.Error);

            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var kaynak = await db.TasinmazKontrolForm_Table.AsNoTracking().FirstOrDefaultAsync(f => f.Id == kaynakFormId, cancellationToken);
                if (kaynak is null) return Result<int>.Failure(Error.NotFound("Kaynak form bulunamadı."));
                if (await db.TasinmazKontrolForm_Table.AnyAsync(f => f.Kod == kaynak.Kod && f.Durum == KontrolFormDurumu.Taslak, cancellationToken))
                    return Result<int>.Failure(Error.Conflict("Bu formun zaten bir taslak sürümü var."));

                var sonSurum = await db.TasinmazKontrolForm_Table.Where(f => f.Kod == kaynak.Kod).MaxAsync(f => f.Surum, cancellationToken);
                var yeni = Kopya(kaynak);
                yeni.Surum = sonSurum + 1;
                yeni.KaynakFormId = kaynak.Id;

                db.TasinmazKontrolForm_Table.Add(yeni);
                await db.SaveChangesAsync(cancellationToken);
                return Result<int>.Success(yeni.Id);
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                return Result<int>.Failure(Error.Conflict("Aynı anda başka bir sürüm oluşturuldu. Tekrar deneyin."));
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.YeniSurumOlusturAsync hata.", ex);
                return Result<int>.Failure(Error.Unexpected("Yeni sürüm oluşturulamadı.", ex));
            }
        }

        public async Task<Result<int>> KopyalaAsync(int kaynakFormId, string yeniKod, string yeniAd, CancellationToken cancellationToken = default)
        {
            var yetki = await CheckAsync(ModuleName.TBYS, Operation.Create, cancellationToken);
            if (yetki is not null) return Result<int>.Failure(yetki.Error);
            if (string.IsNullOrWhiteSpace(yeniKod) || string.IsNullOrWhiteSpace(yeniAd))
                return Result<int>.Failure(Error.Validation("Yeni kod ve ad zorunludur."));

            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var kaynak = await db.TasinmazKontrolForm_Table.AsNoTracking().FirstOrDefaultAsync(f => f.Id == kaynakFormId, cancellationToken);
                if (kaynak is null) return Result<int>.Failure(Error.NotFound("Kaynak form bulunamadı."));

                var kod = yeniKod.Trim().ToUpperInvariant();
                if (await db.TasinmazKontrolForm_Table.AnyAsync(f => f.Kod == kod, cancellationToken))
                    return Result<int>.Failure(Error.Conflict("Bu kodla bir form zaten var."));

                var yeni = Kopya(kaynak);
                yeni.Kod = kod;
                yeni.Ad = yeniAd.Trim();
                yeni.Surum = 1;
                yeni.KaynakFormId = kaynak.Id;

                db.TasinmazKontrolForm_Table.Add(yeni);
                await db.SaveChangesAsync(cancellationToken);
                return Result<int>.Success(yeni.Id);
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                return Result<int>.Failure(Error.Conflict("Bu kodla bir form zaten var."));
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.KopyalaAsync hata.", ex);
                return Result<int>.Failure(Error.Unexpected("Form kopyalanamadı.", ex));
            }
        }

        public async Task<Result> YayimlaAsync(int id, CancellationToken cancellationToken = default)
        {
            var yetki = await CheckAsync(ModuleName.TBYS, Operation.Edit, cancellationToken);
            if (yetki is not null) return yetki;

            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var f = await db.TasinmazKontrolForm_Table.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                if (f is null) return Result.Failure(Error.NotFound("Form bulunamadı."));
                if (f.Durum != KontrolFormDurumu.Taslak) return Result.Failure(Error.Conflict("Yalnızca taslaklar yayımlanabilir."));

                var tanimHatalari = KontrolFormDogrulayici.TanimiDogrula(KontrolFormTanimi.FromJson(f.TanimJson));
                if (tanimHatalari.Count > 0)
                    return Result.Failure(Error.Validation("Form tanımı geçersiz: " + string.Join(" ", tanimHatalari)));

                // Aynı kapsamda (kod farklı olsa bile) başka yayımlı form olmasın: kapsam çözümlemesi tek sonuç versin.
                var cakisan = await db.TasinmazKontrolForm_Table.AnyAsync(x =>
                    x.Id != f.Id && x.Kod != f.Kod && x.Durum == KontrolFormDurumu.Yayimlanmis &&
                    x.Kapsam == f.Kapsam && x.KapsamCinsi == f.KapsamCinsi && x.KapsamTasinmazId == f.KapsamTasinmazId, cancellationToken);
                if (cakisan)
                    return Result.Failure(Error.Conflict($"Aynı kapsamda ({f.KapsamEtiketi}) yayımlanmış başka bir form var."));

                var kullanici = await GetUserNameAsync(cancellationToken);
                var simdi = DateTime.Now;

                var oncekiler = await db.TasinmazKontrolForm_Table
                    .Where(x => x.Kod == f.Kod && x.Id != f.Id && x.Durum == KontrolFormDurumu.Yayimlanmis)
                    .ToListAsync(cancellationToken);
                foreach (var o in oncekiler)
                {
                    o.Durum = KontrolFormDurumu.Arsivlenmis;
                    o.ArsivTarihi = simdi;
                    o.Arsivleyen = kullanici;
                    o.KayitSurumu++;
                }

                f.Durum = KontrolFormDurumu.Yayimlanmis;
                f.YayimTarihi = simdi;
                f.Yayimlayan = kullanici;
                f.KayitSurumu++;

                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Result.Failure(Error.Conflict("Form başka bir kullanıcı tarafından değiştirildi. Sayfayı yenileyin."));
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.YayimlaAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Form yayımlanamadı.", ex));
            }
        }

        public async Task<Result> ArsivleAsync(int id, CancellationToken cancellationToken = default)
        {
            var yetki = await CheckAsync(ModuleName.TBYS, Operation.Edit, cancellationToken);
            if (yetki is not null) return yetki;

            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var f = await db.TasinmazKontrolForm_Table.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                if (f is null) return Result.Failure(Error.NotFound("Form bulunamadı."));
                if (f.Durum == KontrolFormDurumu.Arsivlenmis) return Result.Success();

                f.Durum = KontrolFormDurumu.Arsivlenmis;
                f.ArsivTarihi = DateTime.Now;
                f.Arsivleyen = await GetUserNameAsync(cancellationToken);
                f.KayitSurumu++;
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Result.Failure(Error.Conflict("Form başka bir kullanıcı tarafından değiştirildi. Sayfayı yenileyin."));
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.ArsivleAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Form arşivlenemedi.", ex));
            }
        }

        private static Error? Dogrula(TasinmazKontrolForm? form)
        {
            if (form is null) return Error.Validation("Form boş olamaz.");
            if (string.IsNullOrWhiteSpace(form.Kod)) return Error.Validation("Form kodu zorunludur.");
            if (string.IsNullOrWhiteSpace(form.Ad)) return Error.Validation("Form adı zorunludur.");
            if (form.Kapsam == KontrolFormKapsami.Cins && string.IsNullOrWhiteSpace(form.KapsamCinsi))
                return Error.Validation("Cins kapsamı için cins belirtilmelidir.");
            if (form.Kapsam == KontrolFormKapsami.Tasinmaz && form.KapsamTasinmazId is null or <= 0)
                return Error.Validation("Taşınmaz kapsamı için taşınmaz seçilmelidir.");
            if ((form.VarsayilanPeriyotSure.HasValue) != form.VarsayilanPeriyotBirim.HasValue ||
                form.VarsayilanPeriyotSure is <= 0)
                return Error.Validation("Varsayılan periyot için süre ve birim birlikte, pozitif olarak girilmelidir.");

            try { KontrolFormTanimi.FromJson(form.TanimJson); }
            catch (Exception) { return Error.Validation("Form tanımı okunamadı (geçersiz JSON)."); }
            return null;
        }

        private static TasinmazKontrolForm Kopya(TasinmazKontrolForm k) => new()
        {
            Kod = k.Kod,
            Ad = k.Ad,
            Aciklama = k.Aciklama,
            Kapsam = k.Kapsam,
            KapsamCinsi = k.KapsamCinsi,
            KapsamTasinmazId = k.KapsamTasinmazId,
            Durum = KontrolFormDurumu.Taslak,
            VarsayilanPeriyotSure = k.VarsayilanPeriyotSure,
            VarsayilanPeriyotBirim = k.VarsayilanPeriyotBirim,
            TanimJson = KontrolFormTanimi.FromJson(k.TanimJson).ToJson()
        };

        private static bool IsUniqueViolation(DbUpdateException ex)
            => ex.InnerException is Microsoft.Data.SqlClient.SqlException sql && (sql.Number == 2601 || sql.Number == 2627);
    }
}
