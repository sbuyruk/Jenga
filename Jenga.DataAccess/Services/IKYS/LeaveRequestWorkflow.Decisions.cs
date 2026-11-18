using System.Data;
using System.Globalization;
using System.Net;
using Jenga.DataAccess.Data;
using Jenga.Models.Enums;
using Jenga.Models.IKYS;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;
using Status = Jenga.Models.IKYS.IzinTalep.OnayDurumuTipi;

namespace Jenga.DataAccess.Services.IKYS;

public sealed partial class LeaveRequestWorkflow
{
    public Task<Result<LeaveWriteResult>> DecideAsync(int id, LeaveDecision decision, string? note, bool documentReceived,
        CancellationToken cancellationToken = default) => RunAsync<LeaveWriteResult>("Decide", async db =>
    {
        var ct = cancellationToken;
        var actor = await FindActorAsync(db, true, Operation.Manage, ct);
        if (actor.IsFailure) return actor.Error;
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var request = await FindRequestAsync(db, id, actor.Value, true, ct);
        if (request?.Personel is not { } person) return MissingRequest;

        var next = decision switch
        {
            LeaveDecision.Check when CanEdit(request) => Status.KontrolEdildi,
            LeaveDecision.Reject when CanEdit(request) || request.OnayDurumu == Status.KontrolEdildi => Status.Reddedildi,
            LeaveDecision.Approve when request.OnayDurumu == Status.KontrolEdildi => Status.KayitlaraIslendi,
            _ => (Status?)null
        };
        if (!next.HasValue || await HasMovementAsync(db, id, ct)) return ChangedRequest;
        if (decision == LeaveDecision.Approve && !documentReceived)
            return Error.Validation("İmzalı izin belgesinin teslim alındığını doğrulayınız.");

        IzinDonem? period = null;
        if (decision != LeaveDecision.Reject)
        {
            if (request.BaslangicTarihi is not { } start || request.BitisTarihi is not { } end || end < start || end.Year >= 9999)
                return Error.Validation("Talebin başlangıç ve bitiş tarihlerini kontrol ediniz.");
            var type = (LeaveType?)request.IzinTipi;
            if (type is null || !Enum.IsDefined(type.Value))
                return Error.Validation("Talebin izin türü geçerli değil.");
            if (UsesCalendarDays(type.Value))
            {
                request.Sure = Number(CalendarDays(start, end));
                request.Birim = "Gün";
            }
            if (HasBalance(type.Value))
            {
                period = await db.IzinDonem_Table.FirstOrDefaultAsync(x => x.Id == request.IzinDonemId &&
                    x.PersonelId == request.PersonelId && x.IzinTipi == request.IzinTipi, ct);
                if (period is null || !(period.BaslangicTarihi?.Date <= start.Date && period.BitisTarihi?.Date >= start.Date))
                    return Error.Validation("Talebe ait geçerli izin dönemi bulunamadı.");
            }
            if (decision == LeaveDecision.Approve)
            {
                var applied = await AddMovementAsync(db, request, person, type.Value, period, start, end, ct);
                if (applied.IsFailure) return applied.Error;
            }
        }

        request.OnayDurumu = next;
        if (!string.IsNullOrWhiteSpace(note)) request.Aciklama = note.Trim();
        db.SetCurrentUser(await authorization.GetUserNameAsync(ct));
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        var failed = request.EPostaGonder == true && !await NotifyAsync(request, person, period);
        return new LeaveWriteResult(request.Id, failed);
    }, cancellationToken);

    private static async Task<Result> AddMovementAsync(ApplicationDbContext db, IzinTalep request, Personel person,
        LeaveType type, IzinDonem? period, DateTime start, DateTime end, CancellationToken ct)
    {
        var movement = new IzinHareket
        {
            IzinTalepId = request.Id, PersonelId = person.Id, IzinTipi = request.IzinTipi,
            IzinDonemId = request.IzinDonemId, BaslangicTarihi = start, BitisTarihi = end,
            Sure = request.Sure, Birim = request.Birim, Adres = request.Adres, VekilImza = request.VekilImza,
            AmirImza = request.AmirImza, OnayImza = request.OnayImza, Mahsup = false
        };

        if (period is not null)
        {
            movement.OncekiIzinStr = period.KalanIzin;
            movement.KullanilanIzinStr = request.Sure;
            if (type == LeaveType.Paid)
            {
                var requested = Days(request.Sure);
                var otherBalances = await db.IzinDonem_Table.Where(x => x.Id != period.Id &&
                    x.PersonelId == person.Id && x.IzinTipi == (int)type).Select(x => x.KalanIzin).ToListAsync(ct);
                var used = Days(period.KullanilanIzin);
                var allowance = Days(period.IzinHakki);
                if (requested <= 0 || allowance - used + otherBalances.Sum(Days) < requested)
                    return Result.Failure(Error.Validation("Yıllık izin bakiyesi yetersiz veya talep süresi geçersiz."));
                period.KullanilanIzin = Number(used + requested);
                period.KalanIzin = Number(allowance - used - requested);
            }
            else
            {
                var requested = Hours(request.Sure);
                var used = Hours(period.KullanilanIzin);
                var allowance = Hours(period.IzinHakki);
                if (requested <= TimeSpan.Zero || allowance - used < requested)
                    return Result.Failure(Error.Validation("Mazeret izni bakiyesi yetersiz veya talep süresi geçersiz."));
                period.KullanilanIzin = (used + requested).ToString("c", CultureInfo.InvariantCulture);
                period.KalanIzin = (allowance - used - requested).ToString("c", CultureInfo.InvariantCulture);
            }
            movement.KalanIzinStr = period.KalanIzin;
        }
        else if (type == LeaveType.Unpaid && person.AskerSivil != AskerSivil.Asker)
        {
            var job = await FindJobAsync(db, person.Id, ct);
            var duration = CalendarDays(start, end);
            if (job is null || (job.IzinDonemiBasTar ?? job.BaslamaTar) is not { } entitlementStart ||
                entitlementStart.Date > DateTime.MaxValue.Date.AddDays(-duration))
                return Result.Failure(Error.Validation("Ücretsiz izin için yeni izin dönemi başlangıcı belirlenemedi."));
            job.IzinDonemiBasTar = entitlementStart.AddDays(duration);
        }

        db.IzinHareket_Table.Add(movement);
        return Result.Success();
    }

    private string? DocumentUrl(IzinTalep request, IzinDonem? period)
    {
        if (request.OnayDurumu != Status.KontrolEdildi || SiteUrl("PortalDocumentBaseUrl") is not { } root) return null;
        var isPersonal = request.IzinTipi == (int)LeaveType.Personal;
        var isPaid = request.IzinTipi == (int)LeaveType.Paid;
        var viewer = isPersonal ? "MazeretIzinBelgesiViewer.aspx" : "UcretlIzinBelgesiViewer.aspx";
        var parameters = $"IzinTalepId={request.Id}";

        if (isPaid && period is not null)
        {
            var after = Number(Days(period.KalanIzin) - Days(request.Sure));
            parameters += $"&IzinDonemId={period.Id}&SonIzin={Uri.EscapeDataString(after + " Gün")}";
        }
        else if (isPersonal && period is not null)
        {
            var requested = Hours(request.Sure);
            var balance = Hours(period.KalanIzin);
            parameters += $"&SureStr={Uri.EscapeDataString(DisplayHours(requested))}&KalanIzinStr={Uri.EscapeDataString(DisplayHours(balance))}" +
                $"&SonIzin={Uri.EscapeDataString(DisplayHours(balance - requested))}";
        }
        else
        {
            parameters += $"&SureStr={Uri.EscapeDataString(request.Sure ?? "")}&KalanIzinStr=&SonIzin=";
        }
        return $"{root}/KullaniciUygulamalari/Sayfalar/{viewer}?{parameters}";
    }

    private string? SiteUrl(string key)
    {
        return Uri.TryCreate(configuration[$"LeaveRequests:{key}"], UriKind.Absolute, out var uri) &&
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps) && string.IsNullOrEmpty(uri.Query) &&
            string.IsNullOrEmpty(uri.Fragment) && string.IsNullOrEmpty(uri.UserInfo)
            ? uri.AbsoluteUri.TrimEnd('/') : null;
    }

    private async Task<bool> NotifyAsync(IzinTalep request, Personel person, IzinDonem? period)
    {
        try
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var address = await db.IletisimBilgileri_Table.Where(x => x.PersonelId == person.Id)
                .Select(x => x.InternetEPosta).FirstOrDefaultAsync();
            if (string.IsNullOrWhiteSpace(address) && !string.IsNullOrWhiteSpace(person.KullaniciAdi))
                address = $"{person.KullaniciAdi}@{configuration["LeaveRequests:PersonnelEmailDomain"] ?? "tskgv.local"}";
            var type = await db.IzinTanim_Table.Where(x => x.Id == request.IzinTipi).Select(x => x.Adi).FirstOrDefaultAsync();
            var dates = $"{request.BaslangicTarihi:dd.MM.yyyy} – {request.BitisTarihi:dd.MM.yyyy}";
            var subject = $"{dates} {type ?? "İzin"} talebiniz";
            var body = request.OnayDurumu switch
            {
                Status.KontrolEdildi => "İzin talebiniz kontrol edildi. İmzalı izin belgenizi Personel Kısmına teslim ediniz.",
                Status.Reddedildi => $"İzin talebiniz reddedildi. {WebUtility.HtmlEncode(request.Aciklama)}",
                Status.KayitlaraIslendi => "İzin talebiniz kayıtlara işlendi.",
                _ => "İzin talebiniz oluşturuldu."
            };
            if (DocumentUrl(request, period) is { } document)
                body += $" <a href=\"{WebUtility.HtmlEncode(document)}\">İzin belgesi</a>";
            if (SiteUrl("BaseUrl") is { } site)
                body += $" <a href=\"{WebUtility.HtmlEncode(site)}/ikys/leave-requests\">İzin Taleplerim</a>";

            var personNotified = !string.IsNullOrWhiteSpace(address) && await email.TrySendAsync([address], subject, body);
            if (request.OnayDurumu != Status.IslemBekliyor) return personNotified;

            var group = (configuration["LeaveRequests:NotificationGroup"] ?? "izinonaymailgrubu@tskgv.local")
                .Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (group.Length == 0) return personNotified;
            var officeBody = $"{WebUtility.HtmlEncode($"{person.Adi} {person.Soyadi}")} tarafından {dates} için izin talebi oluşturuldu.";
            if (SiteUrl("BaseUrl") is { } officeSite)
                officeBody += $" <a href=\"{WebUtility.HtmlEncode(officeSite)}/ikys/leave-requests/manage\">İzin Talepleri</a>";
            var officeNotified = await email.TrySendAsync(group, "Yeni izin talebi", officeBody);
            return personNotified && officeNotified;
        }
        catch (Exception ex)
        {
            log.LogException(ex, "LeaveRequestWorkflow.Notify");
            return false;
        }
    }
}
