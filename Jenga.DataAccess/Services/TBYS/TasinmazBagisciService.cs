using Jenga.DataAccess.Data;
using Jenga.Models.TBYS;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.TBYS
{
    public class TasinmazBagisciService : ITasinmazBagisciService
    {
        private const string Source = nameof(TasinmazBagisciService);

        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService _logService;

        public TasinmazBagisciService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        }

        public async Task<Result<List<TasinmazBagisci>>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.TasinmazBagisci_Table.AsNoTracking().ToListAsync(cancellationToken);
                return Result.Success(list);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.GetAllAsync");
                return Result.Failure<List<TasinmazBagisci>>(Error.Unexpected("Bağışçılar getirilemedi.", ex, "TasinmazBagisci.GetAll.Failed"));
            }
        }

        public async Task<Result<TasinmazBagisci>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.TasinmazBagisci_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                return entity is null
                    ? Result.Failure<TasinmazBagisci>(Error.NotFound($"Bağışçı bulunamadı (id:{id}).", code: "TasinmazBagisci.NotFound"))
                    : Result.Success(entity);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.GetByIdAsync");
                return Result.Failure<TasinmazBagisci>(Error.Unexpected("Bağışçı getirilemedi.", ex, "TasinmazBagisci.GetById.Failed"));
            }
        }

        public Task<Result<TasinmazBagisci>> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default)
            => GetByIdAsync(id, cancellationToken);

        public async Task<Result> AddAsync(TasinmazBagisci bagisci, CancellationToken cancellationToken = default)
        {
            if (bagisci is null)
                return Result.Failure(Error.Validation("Bağışçı kaydı boş olamaz.", code: "TasinmazBagisci.Add.Null"));

            var name = $"{(bagisci.Adi ?? string.Empty).Trim()} {(bagisci.Soyadi ?? string.Empty).Trim()}".Trim();
            if (string.IsNullOrWhiteSpace(name))
                return Result.Failure(Error.Validation("Ad/Soyad boş olmamalı.", code: "TasinmazBagisci.Add.NameRequired"));

            try
            {
                var existsRes = await ExistsByTCKimlikAsync(bagisci.TCKimlikNo, null, cancellationToken);
                if (existsRes.IsSuccess && existsRes.Value)
                    return Result.Failure(Error.Conflict($"Aynı TCKimlikNo zaten kayıtlı: '{bagisci.TCKimlikNo}'.", code: "TasinmazBagisci.Add.DuplicateTC"));

                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                await db.TasinmazBagisci_Table.AddAsync(bagisci, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.AddAsync");
                return Result.Failure(Error.Unexpected("Bağışçı eklenemedi.", ex, "TasinmazBagisci.Add.Failed"));
            }
        }

        public async Task<Result> UpdateAsync(TasinmazBagisci bagisci, CancellationToken cancellationToken = default)
        {
            if (bagisci is null)
                return Result.Failure(Error.Validation("Bağışçı kaydı boş olamaz.", code: "TasinmazBagisci.Update.Null"));

            var name = $"{(bagisci.Adi ?? string.Empty).Trim()} {(bagisci.Soyadi ?? string.Empty).Trim()}".Trim();
            if (string.IsNullOrWhiteSpace(name))
                return Result.Failure(Error.Validation("Ad/Soyad boş olmamalı.", code: "TasinmazBagisci.Update.NameRequired"));

            try
            {
                var existsRes = await ExistsByTCKimlikAsync(bagisci.TCKimlikNo, bagisci.Id, cancellationToken);
                if (existsRes.IsSuccess && existsRes.Value)
                    return Result.Failure(Error.Conflict($"Aynı TCKimlikNo zaten kayıtlı: '{bagisci.TCKimlikNo}' (id:{bagisci.Id}).", code: "TasinmazBagisci.Update.DuplicateTC"));

                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.TasinmazBagisci_Table.Update(bagisci);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.UpdateAsync");
                return Result.Failure(Error.Unexpected("Bağışçı güncellenemedi.", ex, "TasinmazBagisci.Update.Failed"));
            }
        }

        public async Task<Result> DeleteAsync(int bagisciId, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

                if (await db.Tasinmaz_Table.AsNoTracking().AnyAsync(t => t.BagisciId == bagisciId, cancellationToken))
                    return Result.Failure(Error.Conflict("Bu bağışçı bir taşınmaz kaydında referans olarak kullanılıyor.", code: "TasinmazBagisci.Delete.Referenced"));

                var entity = await db.TasinmazBagisci_Table.FirstOrDefaultAsync(x => x.Id == bagisciId, cancellationToken);
                if (entity is null)
                    return Result.Failure(Error.NotFound($"Bağışçı bulunamadı (id:{bagisciId}).", code: "TasinmazBagisci.NotFound"));

                db.TasinmazBagisci_Table.Remove(entity);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.DeleteAsync");
                return Result.Failure(Error.Unexpected("Bağışçı silinemedi.", ex, "TasinmazBagisci.Delete.Failed"));
            }
        }

        public async Task<Result<bool>> AnyAsync(Expression<Func<TasinmazBagisci, bool>> predicate, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var any = await db.TasinmazBagisci_Table.AnyAsync(predicate, cancellationToken);
                return Result.Success(any);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.AnyAsync");
                return Result.Failure<bool>(Error.Unexpected("Sorgu çalıştırılamadı.", ex, "TasinmazBagisci.Any.Failed"));
            }
        }

        public async Task<Result<(bool CanDelete, string? Reason)>> CanDeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                if (await db.Tasinmaz_Table.AsNoTracking().AnyAsync(t => t.BagisciId == id, cancellationToken))
                    return Result.Success<(bool CanDelete, string? Reason)>((false, "Bu bağışçı bir taşınmaz kaydında referans olarak kullanılıyor, önce onu kaldırmalısınız."));

                return Result.Success<(bool CanDelete, string? Reason)>((true, null));
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.CanDeleteAsync");
                return Result.Failure<(bool CanDelete, string? Reason)>(Error.Unexpected("Silme kontrolü yapılamadı.", ex, "TasinmazBagisci.CanDelete.Failed"));
            }
        }

        public async Task<Result<bool>> ExistsByTCKimlikAsync(long? tckimlik, int? excludeId = null, CancellationToken cancellationToken = default)
        {
            if (!tckimlik.HasValue || tckimlik.Value == 0)
                return Result.Success(false);

            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var exists = await db.TasinmazBagisci_Table.AsNoTracking().AnyAsync(b =>
                    b.TCKimlikNo.HasValue &&
                    b.TCKimlikNo.Value == tckimlik.Value &&
                    (!excludeId.HasValue || b.Id != excludeId.Value), cancellationToken);
                return Result.Success(exists);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.ExistsByTCKimlikAsync");
                return Result.Failure<bool>(Error.Unexpected("Bağışçı varlık kontrolü yapılamadı.", ex, "TasinmazBagisci.Exists.Failed"));
            }
        }
    }
}
