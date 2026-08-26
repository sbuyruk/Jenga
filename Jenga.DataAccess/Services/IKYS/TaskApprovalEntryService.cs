using Jenga.DataAccess.Data;
using Jenga.Models.Enums;
using Jenga.Models.IKYS;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Jenga.DataAccess.Services.IKYS;

public sealed class TaskApprovalEntryService : ITaskApprovalEntryService
{
    private const string Source = nameof(TaskApprovalEntryService);
    private static readonly CultureInfo TrCulture = new("tr-TR");
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly ILogService _logService;
    private readonly IGorevOnayService _gorevOnayService;

    private static readonly IReadOnlyList<TaskApprovalCountryRateOption> DefaultCountryRates =
    [
        new() { Country = "Türkiye", Currency = "TL", DailyAmount = 1250m },
        new() { Country = "Avrupa", Currency = "EUR", DailyAmount = 150m },
        new() { Country = "İngiltere", Currency = "GBP", DailyAmount = 175m },
        new() { Country = "Avrupa Harici", Currency = "USD", DailyAmount = 200m }
    ];

    private static readonly IReadOnlyList<string> UlasimAraclari = ["", "Uçak", "Otobüs", "Tren", "Vakıf Aracı"];
    private static readonly IReadOnlyList<string> TransferSecenekleri = ["", "Vakıf Aracı", "Toplu Taşıma", "Taksi", "Diğer"];
    private static readonly IReadOnlyList<string> KonaklamaSecenekleri = ["Limit Dahilinde", "Limit Aşımı"];

    public TaskApprovalEntryService(
        IDbContextFactory<ApplicationDbContext> dbFactory,
        ILogService logService,
        IGorevOnayService gorevOnayService)
    {
        _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        _gorevOnayService = gorevOnayService ?? throw new ArgumentNullException(nameof(gorevOnayService));
    }

    public async Task<Result<TaskApprovalEntryLoadResult>> LoadAsync(TaskApprovalEntryLoadRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

            var personOptions = await LoadPersonOptionsAsync(db, request, cancellationToken);
            var perDirOptions = await LoadSignatureOptionsAsync(db, request, forDirector: true, cancellationToken);
            var perUzmOptions = await LoadSignatureOptionsAsync(db, request, forDirector: false, cancellationToken);

            var model = await LoadModelAsync(db, request, personOptions, cancellationToken);
            var selectedPersonName = personOptions.FirstOrDefault(x => x.Id == model.PersonelId)?.Label;
            var harcirahGroup = await LoadHarcirahGroupAsync(db, model.PersonelId, cancellationToken);

            var result = new TaskApprovalEntryLoadResult
            {
                Model = model,
                IsEditMode = model.Id > 0,
                ShowSave = model.Id <= 0,
                ShowUpdate = model.Id > 0 && model.Odendi != true,
                ShowDelete = model.Id > 0 && model.Odendi != true,
                ShowReport = model.Id > 0,
                IsPaid = model.Odendi == true,
                SelectedPersonDisplayName = selectedPersonName,
                HarcirahGroup = harcirahGroup,
                PersonOptions = personOptions,
                PerDirOptions = perDirOptions,
                PerUzmOptions = perUzmOptions,
                UlasimAraclari = UlasimAraclari.ToList(),
                TransferSecenekleri = TransferSecenekleri.ToList(),
                KonaklamaSecenekleri = KonaklamaSecenekleri.ToList(),
                CountryRates = DefaultCountryRates.ToList(),
                HarcirahHesaplansin = true,
                SelectedCountry = ResolveCountry(model.ParaBirimi)
            };

            EnsureDefaults(result.Model, result);
            return Result.Success(result);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.LoadAsync");
            return Result.Failure<TaskApprovalEntryLoadResult>(Error.Unexpected("Görev onay giriş verileri hazırlanamadı.", ex, "TaskApprovalEntry.Load.Failed"));
        }
    }

    public async Task<TaskApprovalCalculationResult> CalculateAsync(TaskApprovalCalculationInput input, CancellationToken cancellationToken = default)
    {
        var result = new TaskApprovalCalculationResult();

        if (!TryCreateDateTime(input.StartDate, input.StartTime, out var start) ||
            !TryCreateDateTime(input.EndDate, input.EndTime, out var end))
            return result;

        result.StartDateTime = start;
        result.EndDateTime = end;

        if (end <= start)
        {
            result.IsNegativeDuration = true;
            result.ExplanationText = "Bitiş tarihi başlangıç tarihinden önce olamaz.";
            return result;
        }

        var diff = end - start;
        var totalMinutes = (int)Math.Floor(diff.TotalMinutes);
        result.DurationDays = totalMinutes / (24 * 60);
        result.DurationHours = (totalMinutes % (24 * 60)) / 60;
        result.DurationMinutes = totalMinutes % 60;
        result.DurationDayText = $"{result.DurationDays} Gün";
        result.DurationHourMinuteText = $"{result.DurationHours} Saat {result.DurationMinutes} Dakika";

        var roundedHours = result.DurationHours + (result.DurationMinutes > 0 ? 1 : 0);
        var remainder = roundedHours == 0 ? 0m : roundedHours > 12 ? 1m : 0.5m;
        result.EarnedDays = result.DurationDays + remainder;
        result.EarnedDayText = $"{result.EarnedDays.ToString("0.##", TrCulture)} gün";

        if (!input.HarcirahHesaplansin)
        {
            result.DailyAllowance = 0m;
            result.Currency = ResolveSeries(input.Country, start).Currency;
            result.TotalAllowance = 0m;
            result.TotalAllowanceText = "0";
            result.ExplanationText = "Harcırah Hesaplanmadı";
            return result;
        }

        var startSeries = ResolveSeries(input.Country, start);
        var endSeries = ResolveSeries(input.Country, end);
        result.Currency = startSeries.Currency;

        if (startSeries.SeriesId == endSeries.SeriesId)
        {
            result.DailyAllowance = startSeries.DailyAmount;
            result.TotalAllowance = result.EarnedDays * startSeries.DailyAmount;
            result.TotalAllowanceText = result.TotalAllowance.ToString("N2", TrCulture);
            result.ExplanationText = $"{startSeries.DailyAmount.ToString("N2", TrCulture)} {startSeries.Currency} x {result.EarnedDayText} = {result.TotalAllowanceText} {startSeries.Currency}";
            return result;
        }

        var splitAt = endSeries.EffectiveStartDate;
        var firstPart = CalculateSegment(start, splitAt, startSeries.DailyAmount);
        var secondPart = CalculateSegment(splitAt, end, endSeries.DailyAmount);

        result.TotalAllowance = firstPart.total + secondPart.total;
        result.TotalAllowanceText = result.TotalAllowance.ToString("N2", TrCulture);
        result.DailyAllowance = startSeries.DailyAmount;
        result.ExplanationText =
            $"{startSeries.DailyAmount.ToString("N2", TrCulture)} {startSeries.Currency} x {firstPart.days.ToString("0.##", TrCulture)} gün = {firstPart.total.ToString("N2", TrCulture)} {startSeries.Currency}{Environment.NewLine}" +
            $"{endSeries.DailyAmount.ToString("N2", TrCulture)} {endSeries.Currency} x {secondPart.days.ToString("0.##", TrCulture)} gün = {secondPart.total.ToString("N2", TrCulture)} {endSeries.Currency}{Environment.NewLine}" +
            $"Toplam Harcırah = {result.TotalAllowanceText} {startSeries.Currency}";

        return result;
    }

    public Task<Result<bool>> HasOverlapAsync(int personelId, DateTime startDate, DateTime endDate, int? excludeTaskApprovalId = null, CancellationToken cancellationToken = default)
        => _gorevOnayService.HasOverlappingTaskApprovalAsync(personelId, startDate, endDate, excludeTaskApprovalId, cancellationToken);

    private static bool TryCreateDateTime(DateTime? date, string? timeText, out DateTime dateTime)
    {
        dateTime = default;
        if (!date.HasValue || !TimeOnly.TryParseExact(timeText, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var time))
            return false;

        dateTime = date.Value.Date + time.ToTimeSpan();
        return true;
    }

    private static (decimal days, decimal total) CalculateSegment(DateTime start, DateTime end, decimal dailyAmount)
    {
        if (end <= start)
            return (0m, 0m);

        var totalMinutes = (int)Math.Floor((end - start).TotalMinutes);
        var days = totalMinutes / (24 * 60);
        var hours = (totalMinutes % (24 * 60)) / 60;
        var minutes = totalMinutes % 60;
        var roundedHours = hours + (minutes > 0 ? 1 : 0);
        var remainder = roundedHours == 0 ? 0m : roundedHours > 12 ? 1m : 0.5m;
        var earnedDays = days + remainder;
        return (earnedDays, earnedDays * dailyAmount);
    }

    private static string ResolveCountry(string? currency)
    {
        var found = DefaultCountryRates.FirstOrDefault(x => string.Equals(x.Currency, currency, StringComparison.OrdinalIgnoreCase));
        return found?.Country ?? DefaultCountryRates[0].Country;
    }

    private static RateSeries ResolveSeries(string country, DateTime date)
    {
        var normalized = country.Trim();
        var series = BuildSeries(normalized);
        return series
            .Where(x => x.EffectiveStartDate <= date)
            .OrderByDescending(x => x.EffectiveStartDate)
            .First();
    }

    private static List<RateSeries> BuildSeries(string country)
    {
        return country switch
        {
            "Avrupa" => [new(1, new DateTime(2024, 1, 1), 140m, "EUR"), new(2, new DateTime(2025, 1, 1), 150m, "EUR")],
            "İngiltere" => [new(1, new DateTime(2024, 1, 1), 165m, "GBP"), new(2, new DateTime(2025, 1, 1), 175m, "GBP")],
            "Avrupa Harici" => [new(1, new DateTime(2024, 1, 1), 185m, "USD"), new(2, new DateTime(2025, 1, 1), 200m, "USD")],
            _ => [new(1, new DateTime(2024, 1, 1), 1100m, "TL"), new(2, new DateTime(2025, 1, 1), 1250m, "TL")]
        };
    }

    private static void EnsureDefaults(GorevOnay model, TaskApprovalEntryLoadResult context)
    {
        if (model.PersonelId is null && context.PersonOptions.Count > 0)
            model.PersonelId = context.PersonOptions[0].Id;

        if (string.IsNullOrWhiteSpace(model.UlasimAraci))
            model.UlasimAraci = context.UlasimAraclari.FirstOrDefault() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(model.Transfer))
            model.Transfer = context.TransferSecenekleri.FirstOrDefault() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(model.Konaklama))
            model.Konaklama = context.KonaklamaSecenekleri.FirstOrDefault() ?? "Limit Dahilinde";
        if (string.IsNullOrWhiteSpace(model.ParaBirimi))
            model.ParaBirimi = DefaultCountryRates[0].Currency;
    }

    private static bool IsAuth(string? auth, string expected)
        => string.Equals(auth, expected, StringComparison.OrdinalIgnoreCase);

    private async Task<List<TaskApprovalOptionItem>> LoadPersonOptionsAsync(
        ApplicationDbContext db,
        TaskApprovalEntryLoadRequest request,
        CancellationToken cancellationToken)
    {
        if (!request.CurrentPersonelId.HasValue)
            return [];

        if (request.CanManage && IsAuth(request.Auth, "IKYS"))
        {
            return await db.Personel_Table.AsNoTracking()
                .Join(
                    db.IsBilgileri_Table.AsNoTracking().Where(x => x.CalismaDurumu == CalismaDurumu.Calisiyor),
                    p => p.Id,
                    ib => ib.PersonelId,
                    (p, _) => new TaskApprovalOptionItem { Id = p.Id, Label = (p.Adi + " " + p.Soyadi).Trim() })
                .Distinct()
                .OrderBy(x => x.Label)
                .ToListAsync(cancellationToken);
        }

        if (IsAuth(request.Auth, "BIRIM"))
        {
            var birimId = request.QueryBirimTanimId;
            if (!birimId.HasValue)
            {
                birimId = await db.IsBilgileri_Table.AsNoTracking()
                    .Where(x => x.PersonelId == request.CurrentPersonelId.Value)
                    .Select(x => x.BirimId)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            if (!birimId.HasValue)
                return [];

            var birimIds = await GetChildBirimIdsAsync(db, birimId.Value, cancellationToken);
            return await db.Personel_Table.AsNoTracking()
                .Join(
                    db.IsBilgileri_Table.AsNoTracking().Where(x => x.CalismaDurumu == CalismaDurumu.Calisiyor && x.BirimId.HasValue && birimIds.Contains(x.BirimId.Value)),
                    p => p.Id,
                    ib => ib.PersonelId,
                    (p, _) => new TaskApprovalOptionItem { Id = p.Id, Label = (p.Adi + " " + p.Soyadi).Trim() })
                .Distinct()
                .OrderBy(x => x.Label)
                .ToListAsync(cancellationToken);
        }

        var current = await db.Personel_Table.AsNoTracking()
            .Where(x => x.Id == request.CurrentPersonelId.Value)
            .Select(x => new TaskApprovalOptionItem { Id = x.Id, Label = (x.Adi + " " + x.Soyadi).Trim() })
            .FirstOrDefaultAsync(cancellationToken);

        return current is null ? [] : [current];
    }

    private async Task<List<TaskApprovalOptionItem>> LoadSignatureOptionsAsync(
        ApplicationDbContext db,
        TaskApprovalEntryLoadRequest request,
        bool forDirector,
        CancellationToken cancellationToken)
    {
        var query = from ib in db.IsBilgileri_Table.AsNoTracking()
                    join p in db.Personel_Table.AsNoTracking() on ib.PersonelId equals p.Id
                    join u in db.UnvanTanim_Table.AsNoTracking() on ib.UnvanId equals u.Id into uJoin
                    from u in uJoin.DefaultIfEmpty()
                    where ib.CalismaDurumu == CalismaDurumu.Calisiyor
                    select new
                    {
                        p.Id,
                        Label = (p.Adi + " " + p.Soyadi).Trim(),
                        Title = u != null ? u.Adi : null
                    };

        var list = await query.ToListAsync(cancellationToken);
        IEnumerable<TaskApprovalOptionItem> filtered = list
            .Where(x => !string.IsNullOrWhiteSpace(x.Title))
            .Where(x => forDirector
                ? ContainsAny(x.Title!, "DİREKTÖR", "DIREKTOR", "BAŞUZMAN", "BASUZMAN", "KIDEMLİ UZMAN", "KIDEMLI UZMAN")
                : ContainsAny(x.Title!, "UZMAN", "KIDEMLİ UZMAN", "KIDEMLI UZMAN"))
            .Select(x => new TaskApprovalOptionItem { Id = x.Id, Label = x.Label })
            .DistinctBy(x => x.Id)
            .OrderBy(x => x.Label);

        var options = filtered.ToList();
        if (options.Count > 0)
            return options;

        return list.Select(x => new TaskApprovalOptionItem { Id = x.Id, Label = x.Label })
            .DistinctBy(x => x.Id)
            .OrderBy(x => x.Label)
            .Take(100)
            .ToList();
    }

    private async Task<GorevOnay> LoadModelAsync(
        ApplicationDbContext db,
        TaskApprovalEntryLoadRequest request,
        IReadOnlyCollection<TaskApprovalOptionItem> personOptions,
        CancellationToken cancellationToken)
    {
        if (request.GorevOnayId.HasValue)
        {
            var model = await db.GorevOnay_Table.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.GorevOnayId.Value, cancellationToken);
            if (model is not null)
                return model;
        }

        var selectedPersonId = request.QueryPersonelId
            ?? personOptions.FirstOrDefault()?.Id
            ?? request.CurrentPersonelId;

        var now = DateTime.Now;
        return new GorevOnay
        {
            PersonelId = selectedPersonId,
            BaslangicTarihi = now.Date.AddHours(8).AddMinutes(30),
            BitisTarihi = now.Date.AddHours(17),
            ParaBirimi = "TL",
            Transfer = TransferSecenekleri[0],
            UlasimAraci = UlasimAraclari[0],
            Konaklama = KonaklamaSecenekleri[0],
            AmirOnayi = (int)ApprovalStatus.PendingApproval
        };
    }

    private async Task<string> LoadHarcirahGroupAsync(ApplicationDbContext db, int? personelId, CancellationToken cancellationToken)
    {
        if (!personelId.HasValue)
            return "Standart";

        var group = await db.GorevTanim_Table.AsNoTracking()
            .Where(x => x.PersonelId == personelId.Value)
            .OrderByDescending(x => x.OlusturmaTarihi)
            .Select(x => x.Adi)
            .FirstOrDefaultAsync(cancellationToken);

        return string.IsNullOrWhiteSpace(group) ? "Standart" : group;
    }

    private static bool ContainsAny(string source, params string[] tokens)
        => tokens.Any(token => source.Contains(token, StringComparison.OrdinalIgnoreCase));

    private static async Task<HashSet<int>> GetChildBirimIdsAsync(ApplicationDbContext db, int rootBirimId, CancellationToken cancellationToken)
    {
        var all = await db.BirimTanim_Table.AsNoTracking()
            .Where(x => x.Aktif == true)
            .Select(x => new { x.Id, x.ParentId })
            .ToListAsync(cancellationToken);

        var result = new HashSet<int> { rootBirimId };
        var queue = new Queue<int>();
        queue.Enqueue(rootBirimId);

        while (queue.Count > 0)
        {
            var parent = queue.Dequeue();
            foreach (var child in all.Where(x => x.ParentId == parent).Select(x => x.Id))
            {
                if (result.Add(child))
                    queue.Enqueue(child);
            }
        }

        return result;
    }

    private sealed record RateSeries(int SeriesId, DateTime EffectiveStartDate, decimal DailyAmount, string Currency);
}
