using Jenga.DataAccess.Data;
using Jenga.Models.TBYS;
using Jenga.Models.TBYS.Search;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.TBYS
{
    /// <summary>
    /// TBYS'e özgü detay sorgularını içerir.
    /// Genel (global) arama için GlobalSearchService kullanılır.
    /// </summary>
    public class TbysSearchService : ITbysSearchService
    {
        private const string Source = nameof(TbysSearchService);

        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService _logService;

        public TbysSearchService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        }

        public async Task<Result<KiraciDetayVM>> GetKiraciDetayAsync(int kiraciId, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

                var kiraci = await db.Kiraci_Table.AsNoTracking()
                    .FirstOrDefaultAsync(k => k.Id == kiraciId, cancellationToken);

                if (kiraci is null)
                    return Result<KiraciDetayVM>.Failure(Error.NotFound("Kiracı bulunamadı."));

                var sozlesmeler = await db.KiraSozlesme_Table.AsNoTracking()
                    .Where(s => s.KiraciId == kiraciId)
                    .OrderByDescending(s => s.SozBasTar)
                    .ToListAsync(cancellationToken);

                var sozlesmeIds = sozlesmeler.Select(s => s.Id).ToList();

                var sozlesmeTasinmazlar = await db.SozlesmeTasinmaz_Table
                    .AsNoTracking()
                    .Where(st => st.SozlesmeId != null && sozlesmeIds.Contains(st.SozlesmeId.Value))
                    .ToListAsync(cancellationToken);

                var tasinmazIds = sozlesmeTasinmazlar
                    .Where(st => st.TasinmazId.HasValue)
                    .Select(st => st.TasinmazId!.Value)
                    .Distinct()
                    .ToList();

                var tasinmazlar = await db.Tasinmaz_Table.AsNoTracking()
                    .Where(t => tasinmazIds.Contains(t.Id))
                    .ToListAsync(cancellationToken);

                var odemePlanlari = await db.OdemePlani_Table.AsNoTracking()
                    .Where(op => op.SozlesmeId != null && sozlesmeIds.Contains(op.SozlesmeId.Value))
                    .ToListAsync(cancellationToken);

                var vm = new KiraciDetayVM
                {
                    Kiraci = kiraci,
                    Sozlesmeler = sozlesmeler.Select(s => new KiraSozlesmeDetayVM
                    {
                        Sozlesme = s,
                        Tasinmazlar = sozlesmeTasinmazlar
                            .Where(st => st.SozlesmeId == s.Id && st.TasinmazId.HasValue)
                            .Select(st => tasinmazlar.FirstOrDefault(t => t.Id == st.TasinmazId!.Value)!)
                            .Where(t => t is not null)
                            .ToList(),
                        OdemePlanlari = odemePlanlari
                            .Where(op => op.SozlesmeId == s.Id)
                            .OrderBy(op => op.VadeBasTar)
                            .ToList()
                    }).ToList()
                };

                return Result<KiraciDetayVM>.Success(vm);
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.GetKiraciDetayAsync hata.", ex);
                return Result<KiraciDetayVM>.Failure(Error.Unexpected("Kiracı detayı alınamadı.", ex));
            }
        }

        public async Task<Result<TasinmazDetayVM>> GetTasinmazDetayAsync(int tasinmazId, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

                var tasinmaz = await db.Tasinmaz_Table.AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == tasinmazId, cancellationToken);

                if (tasinmaz is null)
                    return Result<TasinmazDetayVM>.Failure(Error.NotFound("Taşınmaz bulunamadı."));

                TasinmazBagisci? bagisci = null;
                if (tasinmaz.BagisciId.HasValue)
                {
                    bagisci = await db.TasinmazBagisci_Table.AsNoTracking()
                        .FirstOrDefaultAsync(b => b.Id == tasinmaz.BagisciId.Value, cancellationToken);
                }

                // Sözleşme - Taşınmaz bağlantılarını bul
                var sozlesmeTasinmazlar = await db.SozlesmeTasinmaz_Table.AsNoTracking()
                    .Where(st => st.TasinmazId == tasinmazId)
                    .ToListAsync(cancellationToken);

                var sozlesmeIds = sozlesmeTasinmazlar
                    .Where(st => st.SozlesmeId.HasValue)
                    .Select(st => st.SozlesmeId!.Value)
                    .Distinct()
                    .ToList();

                var sozlesmeler = await db.KiraSozlesme_Table.AsNoTracking()
                    .Where(s => sozlesmeIds.Contains(s.Id))
                    .OrderByDescending(s => s.SozBasTar)
                    .ToListAsync(cancellationToken);

                // Her sözleşmenin diğer taşınmazlarını da getir
                var tumSozlesmeTasinmazlar = await db.SozlesmeTasinmaz_Table.AsNoTracking()
                    .Where(st => st.SozlesmeId != null && sozlesmeIds.Contains(st.SozlesmeId.Value))
                    .ToListAsync(cancellationToken);

                var tumTasinmazIds = tumSozlesmeTasinmazlar
                    .Where(st => st.TasinmazId.HasValue)
                    .Select(st => st.TasinmazId!.Value)
                    .Distinct()
                    .ToList();

                var sozlesmeTasinmazDetay = await db.Tasinmaz_Table.AsNoTracking()
                    .Where(t => tumTasinmazIds.Contains(t.Id))
                    .ToListAsync(cancellationToken);

                var odemePlanlari = await db.OdemePlani_Table.AsNoTracking()
                    .Where(op => op.SozlesmeId != null && sozlesmeIds.Contains(op.SozlesmeId.Value))
                    .ToListAsync(cancellationToken);

                var vm = new TasinmazDetayVM
                {
                    Tasinmaz = tasinmaz,
                    Bagisci = bagisci,
                    Sozlesmeler = sozlesmeler.Select(s => new KiraSozlesmeDetayVM
                    {
                        Sozlesme = s,
                        Tasinmazlar = tumSozlesmeTasinmazlar
                            .Where(st => st.SozlesmeId == s.Id && st.TasinmazId.HasValue)
                            .Select(st => sozlesmeTasinmazDetay.FirstOrDefault(t => t.Id == st.TasinmazId!.Value)!)
                            .Where(t => t is not null)
                            .ToList(),
                        OdemePlanlari = odemePlanlari
                            .Where(op => op.SozlesmeId == s.Id)
                            .OrderBy(op => op.VadeBasTar)
                            .ToList()
                    }).ToList()
                };

                return Result<TasinmazDetayVM>.Success(vm);
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.GetTasinmazDetayAsync hata.", ex);
                return Result<TasinmazDetayVM>.Failure(Error.Unexpected("Taşınmaz detayı alınamadı.", ex));
            }
        }

        public async Task<Result<BagisciDetayVM>> GetBagisciDetayAsync(int bagisciId, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

                var bagisci = await db.TasinmazBagisci_Table.AsNoTracking()
                    .FirstOrDefaultAsync(b => b.Id == bagisciId, cancellationToken);

                if (bagisci is null)
                    return Result<BagisciDetayVM>.Failure(Error.NotFound("Bağışçı bulunamadı."));

                var tasinmazlar = await db.Tasinmaz_Table.AsNoTracking()
                    .Where(t => t.BagisciId == bagisciId)
                    .OrderBy(t => t.Ili)
                    .ToListAsync(cancellationToken);

                var yakinlar = await db.BagisciYakinlari_Table.AsNoTracking()
                    .Where(y => y.BagisciId == bagisciId)
                    .OrderBy(y => y.Sira)
                    .ToListAsync(cancellationToken);

                var talepler = await db.BagisciTalepleri_Table.AsNoTracking()
                    .Where(t => t.BagisciId == bagisciId)
                    .ToListAsync(cancellationToken);

                var vm = new BagisciDetayVM
                {
                    Bagisci    = bagisci,
                    Tasinmazlar = tasinmazlar,
                    Yakinlar   = yakinlar,
                    Talepler   = talepler
                };

                return Result<BagisciDetayVM>.Success(vm);
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.GetBagisciDetayAsync hata.", ex);
                return Result<BagisciDetayVM>.Failure(Error.Unexpected("Bağışçı detayı alınamadı.", ex));
            }
        }
    }
}
