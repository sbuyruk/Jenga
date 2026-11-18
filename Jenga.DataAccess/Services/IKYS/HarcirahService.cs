using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Jenga.DataAccess.Data;
using Jenga.DataAccess.Services.Common;
using Jenga.Models.Enums;
using Jenga.Models.IKYS;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.IKYS;

public sealed class HarcirahService(IDbContextFactory<ApplicationDbContext> factory, IServiceAuthorizationContext authorization, ILogService log) : IHarcirahService
{
    private static readonly StringComparer CountryComparer = StringComparer.OrdinalIgnoreCase;
    private static readonly Error Forbidden = Error.Forbidden("Harcırah tanımları için İKYS yönetim yetkisi gereklidir.");
    private static readonly Error Changed = Error.Conflict("Kayıt başka bir işlemle değiştirildi. Listeyi yenileyip tekrar deneyin.");
    private Task<bool> CanManageAsync(CancellationToken ct) => authorization.HasPermissionAsync(ModuleName.IKYS, Operation.Manage, ct);

    public async Task<Result<HarcirahPageData>> GetAsync(string? country = null, int? seriesId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await CanManageAsync(cancellationToken)) return Result.Failure<HarcirahPageData>(Forbidden);
            await using var db = await factory.CreateDbContextAsync(cancellationToken);
            var all = await db.Harcirah_Table.AsNoTracking().ToListAsync(cancellationToken);
            var countries = all.Select(x => x.Ulke.Trim()).Where(x => x.Length > 0).Distinct(CountryComparer)
                .OrderBy(x => x, StringComparer.Create(new CultureInfo("tr-TR"), true)).ToList();
            var selected = string.IsNullOrWhiteSpace(country)
                ? countries.FirstOrDefault(x => CountryComparer.Equals(x, "Türkiye")) ?? countries.FirstOrDefault() ?? ""
                : countries.FirstOrDefault(x => CountryComparer.Equals(x, country.Trim()));
            if (selected is null) return Result.Failure<HarcirahPageData>(Error.Validation("Seçilen ülke tanımlı değil."));
            var rows = all.Where(x => CountryComparer.Equals(x.Ulke.Trim(), selected)).ToList();
            var series = rows.Where(x => x.SeriId > 0).GroupBy(x => x.SeriId).OrderByDescending(x => x.Key)
                .Select(x => new HarcirahSeriesOption(x.Key, x.Min(r => r.BaslangicTarihi), x.Any(r => !r.BitisTarihi.HasValue) ? null : x.Max(r => r.BitisTarihi))).ToList();
            var selectedSeries = seriesId ?? series.FirstOrDefault()?.Id;
            if (seriesId.HasValue && !series.Any(x => x.Id == seriesId))
                return Result.Failure<HarcirahPageData>(Error.Validation("Seçilen tarih serisi bu ülkeye ait değil."));
            return Result.Success(new HarcirahPageData
            {
                Countries = countries, Country = selected, Series = series, SelectedSeriesId = selectedSeries,
                Undo = (await GetUndoStateAsync(db, rows, selectedSeries, cancellationToken)).Availability,
                Rows = rows.Where(x => !selectedSeries.HasValue || x.SeriId == selectedSeries).OrderBy(x => x.KadroGrupId).ThenBy(x => x.Sira).ThenBy(x => x.Id).ToList()
            });
        }
        catch (Exception ex)
        {
            log.LogException(ex, "Harcirah.GetAsync");
            return Result.Failure<HarcirahPageData>(Error.Unexpected("Harcırah tanımları yüklenemedi. Lütfen tekrar deneyin.", ex));
        }
    }

    public async Task<Result> UpdateAsync(HarcirahUpdateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await CanManageAsync(cancellationToken)) return Result.Failure(Forbidden);
            if (request.Amount < 0 || request.Amount > 9999999999999999.99m || decimal.Round(request.Amount, 2) != request.Amount)
                return Result.Failure(Error.Validation("Miktar sıfır veya pozitif olmalı ve en fazla iki ondalık basamak içermelidir."));
            await using var db = await factory.CreateDbContextAsync(cancellationToken);
            var row = await db.Harcirah_Table.SingleOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (row is null) return Result.Failure(Error.NotFound("Harcırah kaydı bulunamadı."));
            if (row.Miktar != request.OriginalAmount || row.Aciklama != request.OriginalDescription) return Result.Failure(Changed);
            row.Miktar = request.Amount;
            row.Aciklama = request.Description;
            db.SetCurrentUser(await authorization.GetUserNameAsync(cancellationToken));
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException) { return Result.Failure(Changed); }
        catch (Exception ex)
        {
            log.LogException(ex, "Harcirah.UpdateAsync");
            return Result.Failure(Error.Unexpected("Harcırah kaydı güncellenemedi. Lütfen tekrar deneyin.", ex));
        }
    }

    public async Task<Result<HarcirahUndoSeriesResult>> UndoSeriesAsync(HarcirahUndoSeriesRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await CanManageAsync(cancellationToken)) return Result.Failure<HarcirahUndoSeriesResult>(Forbidden);
            await using var db = await factory.CreateDbContextAsync(cancellationToken);
            await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            var all = await db.Harcirah_Table.ToListAsync(cancellationToken);
            var rows = all.Where(x => CountryComparer.Equals(x.Ulke.Trim(), request.Country?.Trim())).ToList();
            var state = await GetUndoStateAsync(db, rows, request.SeriesId, cancellationToken);
            if (!state.Availability.CanUndo) return Result.Failure<HarcirahUndoSeriesResult>(Error.Validation(state.Availability.Message));
            if (string.IsNullOrEmpty(request.Version) || request.Version != state.Availability.Version)
                return Result.Failure<HarcirahUndoSeriesResult>(Changed);
            foreach (var previous in state.Previous) previous.BitisTarihi = null;
            db.Harcirah_Table.RemoveRange(state.Current);
            db.SetCurrentUser(await authorization.GetUserNameAsync(cancellationToken));
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Result.Success(new HarcirahUndoSeriesResult(state.Availability.PreviousSeriesId!.Value, state.Current.Count));
        }
        catch (DbUpdateConcurrencyException) { return Result.Failure<HarcirahUndoSeriesResult>(Changed); }
        catch (Exception ex)
        {
            log.LogException(ex, "Harcirah.UndoSeriesAsync");
            return Result.Failure<HarcirahUndoSeriesResult>(Error.Unexpected("Dönem geri alınamadı. Hiçbir değişiklik kaydedilmedi.", ex));
        }
    }

    private sealed record UndoState(HarcirahSeriesUndoAvailability Availability, List<Harcirah> Current, List<Harcirah> Previous);

    private static async Task<UndoState> GetUndoStateAsync(ApplicationDbContext db, List<Harcirah> rows, int? seriesId, CancellationToken ct)
    {
        UndoState Unavailable(string message) => new(new(false, message), [], []);
        if (!seriesId.HasValue || rows.Count == 0) return Unavailable("Geri alınacak dönemi seçin.");
        if (seriesId != rows.Max(x => x.SeriId)) return Unavailable("Yalnızca seçilen ülkenin son dönemi geri alınabilir.");
        var current = rows.Where(x => x.SeriId == seriesId).ToList();
        if (current.Count == 0 || current.Select(x => x.BaslangicTarihi.Date).Distinct().Count() != 1 ||
            current.Any(x => x.BitisTarihi.HasValue || x.KadroGrupId <= 0) || current.Select(x => x.KadroGrupId).Distinct().Count() != current.Count)
            return Unavailable("Dönem tanımları tutarsız olduğu için geri alınamıyor. Tanımları kontrol edin.");
        var start = current[0].BaslangicTarihi.Date;
        if (start <= DateTime.Today) return Unavailable("Başlamış veya geçmiş dönemler geri alınamaz.");
        var previousId = rows.Where(x => x.SeriId < seriesId && x.SeriId > 0).Select(x => (int?)x.SeriId).Max();
        if (!previousId.HasValue) return Unavailable("Yeniden geçerli olacak önceki dönem bulunamadı.");
        var previous = rows.Where(x => x.SeriId == previousId).ToList();
        if (previous.Any(x => x.BaslangicTarihi.Date >= start || x.BitisTarihi?.Date != start.AddDays(-1)) ||
            previous.Select(x => x.KadroGrupId).Distinct().Count() != previous.Count ||
            previous.Any(p => current.All(c => c.KadroGrupId != p.KadroGrupId)) ||
            rows.Any(x => x.SeriId < previousId && (!x.BitisTarihi.HasValue || x.BitisTarihi.Value.Date >= start)))
            return Unavailable("Önceki dönemin tarihleri güvenle yeniden açılamıyor. Tanımları kontrol edin.");
        // Tasks do not reference a tariff ID. Unknown dates and all tasks extending into the period block undo, regardless of currency or approval.
        if (await db.GorevOnay_Table.AsNoTracking().AnyAsync(x => !x.BaslangicTarihi.HasValue || !x.BitisTarihi.HasValue ||
            x.BaslangicTarihi >= start || x.BitisTarihi >= start, ct))
            return Unavailable("Dönem başlangıcında veya sonrasında görev kaydı ya da tarihleri eksik görev kaydı bulunduğu için geri alınamaz.");
        var snapshot = JsonSerializer.Serialize(rows.OrderBy(x => x.Id).Select(x => new
        {
            x.Id, x.SeriId, x.KadroGrupId, x.KadroGrubu, x.Sira, x.Ulke, x.ParaBirimi, x.BaslangicTarihi, x.BitisTarihi,
            x.Miktar, x.Aciklama, x.OlusturmaTarihi, x.DegistirmeTarihi
        }));
        var version = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(snapshot)));
        return new(new(true, "Görev kaydı bulunmayan bu gelecek dönem geri alınabilir. Önceki dönem yeniden geçerli olur.", previousId, version), current, previous);
    }

    public async Task<Result<HarcirahCreateSeriesResult>> CreateSeriesAsync(HarcirahCreateSeriesRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await CanManageAsync(cancellationToken)) return Result.Failure<HarcirahCreateSeriesResult>(Forbidden);
            var start = request.StartDate.Date;
            if (start <= new DateTime(1753, 1, 1)) return Result.Failure<HarcirahCreateSeriesResult>(Error.Validation("Geçerli bir başlangıç tarihi seçin."));
            await using var db = await factory.CreateDbContextAsync(cancellationToken);
            // Close the prior series and create every group atomically; concurrent attempts must not duplicate a period.
            await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            var all = await db.Harcirah_Table.ToListAsync(cancellationToken);
            var rows = all.Where(x => CountryComparer.Equals(x.Ulke.Trim(), request.Country?.Trim())).ToList();
            if (rows.Count == 0 || !rows.Any(x => x.KadroGrupId > 0))
                return Result.Failure<HarcirahCreateSeriesResult>(Error.Validation("Seçili ülke için kadro grubu tanımı bulunamadı."));
            var latest = rows.Max(x => x.SeriId);
            if (latest != request.ExpectedLatestSeriesId) return Result.Failure<HarcirahCreateSeriesResult>(Changed);
            if (latest == int.MaxValue || start <= rows.Max(x => x.BaslangicTarihi.Date))
                return Result.Failure<HarcirahCreateSeriesResult>(Error.Validation("Yeni başlangıç tarihi mevcut tüm dönemlerin başlangıcından sonra olmalıdır."));
            var templates = rows.Where(x => x.KadroGrupId > 0).OrderByDescending(x => x.SeriId).ThenByDescending(x => x.BaslangicTarihi).ThenByDescending(x => x.Id)
                .GroupBy(x => x.KadroGrupId).Select(x => x.First()).ToList();
            var country = templates[0].Ulke.Trim();
            var currency = CurrencyFor(country, templates.Select(x => x.ParaBirimi));
            if (currency is null) return Result.Failure<HarcirahCreateSeriesResult>(Error.Validation("Seçili ülkenin para birimi tanımı belirsiz."));
            foreach (var previous in rows.Where(x => x.SeriId == latest)) previous.BitisTarihi = start.AddDays(-1);
            var newSeries = Math.Max(latest + 1, 1);
            foreach (var template in templates)
                db.Harcirah_Table.Add(new Harcirah
                {
                    KadroGrupId = template.KadroGrupId, KadroGrubu = template.KadroGrubu, Sira = template.Sira,
                    Ulke = country, ParaBirimi = currency, SeriId = newSeries, BaslangicTarihi = start,
                    BitisTarihi = null, Miktar = 0
                });
            db.SetCurrentUser(await authorization.GetUserNameAsync(cancellationToken));
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Result.Success(new HarcirahCreateSeriesResult(newSeries, templates.Count));
        }
        catch (DbUpdateConcurrencyException) { return Result.Failure<HarcirahCreateSeriesResult>(Changed); }
        catch (Exception ex)
        {
            log.LogException(ex, "Harcirah.CreateSeriesAsync");
            return Result.Failure<HarcirahCreateSeriesResult>(Error.Unexpected("Yeni tarih aralığı oluşturulamadı. Hiçbir değişiklik kaydedilmedi.", ex));
        }
    }

    private static string? CurrencyFor(string country, IEnumerable<string> existing)
    {
        var normalized = new string(country.ToUpperInvariant().Replace('İ', 'I').Replace('Ü', 'U').Replace('Ş', 'S').Where(char.IsLetterOrDigit).ToArray());
        var currencies = existing.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct(CountryComparer).ToList();
        return normalized switch
        {
            "TURKIYE" => "TL", "INGILTERE" or "UNITEDKINGDOM" or "UK" => "Sterlin",
            "AVRUPA" => "Euro", "AVRUPAHARICI" or "AVRUPADISI" => "USD", _ => currencies.Count == 1 ? currencies[0] : null
        };
    }
}
