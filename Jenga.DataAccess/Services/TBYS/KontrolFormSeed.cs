using Jenga.DataAccess.Data;
using Jenga.Models.TBYS.Kontrol;
using Jenga.Utility.Logging;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.TBYS
{
    /// <summary>
    /// Excel'den türetilen varsayılan kontrol formunu (TASINMAZ-KONTROL v1) idempotent biçimde ekler.
    /// Aynı kodlu herhangi bir sürüm varsa hiçbir şey yapmaz; yayımlanmış içerik asla değiştirilmez.
    /// </summary>
    public static class KontrolFormSeed
    {
        public static async Task UygulaAsync(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
                if (!await db.Database.CanConnectAsync(cancellationToken)) return;
                if (await db.TasinmazKontrolForm_Table.AnyAsync(f => f.Kod == VarsayilanKontrolFormu.Kod, cancellationToken)) return;

                var tanim = VarsayilanKontrolFormu.Olustur();
                var hatalar = KontrolFormDogrulayici.TanimiDogrula(tanim).ToList();
                if (hatalar.Count > 0)
                {
                    logService.LogError($"{nameof(KontrolFormSeed)}: varsayılan form geçersiz: {string.Join(" | ", hatalar)}");
                    return;
                }

                var simdi = DateTime.Now;
                db.TasinmazKontrolForm_Table.Add(new TasinmazKontrolForm
                {
                    Kod = VarsayilanKontrolFormu.Kod,
                    Ad = VarsayilanKontrolFormu.Ad,
                    Surum = 1,
                    Kapsam = KontrolFormKapsami.Genel,
                    Durum = KontrolFormDurumu.Yayimlanmis,
                    VarsayilanPeriyotSure = 1,
                    VarsayilanPeriyotBirim = PeriyotBirimi.Yil,
                    TanimJson = tanim.ToJson(),
                    YayimTarihi = simdi,
                    Yayimlayan = "system",
                    Olusturan = "system",
                    OlusturmaTarihi = simdi,
                    Aciklama = "docs/tasinmaz-kontrol-formu.xlsx kaynağından otomatik oluşturuldu."
                });
                await db.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                // Açılışı engellemez; şema henüz uygulanmamış olabilir.
                logService.LogError($"{nameof(KontrolFormSeed)} uygulanamadı.", ex);
            }
        }
    }
}
