using System.Globalization;
using Jenga.Models.IKYS;
using Status = Jenga.Models.IKYS.IzinTalep.OnayDurumuTipi;

namespace Jenga.BlazorUI.Components.IKYS;

internal static class LeaveRequestDisplay
{
    public static string FormatDate(DateTime? date, int? type) =>
        date?.ToString(type is (int)LeaveType.Personal or (int)LeaveType.Nursing ? "dd.MM.yyyy HH:mm" : "dd.MM.yyyy") ?? "—";

    public static string DurationText(string? duration, string? unit)
    {
        if (string.IsNullOrWhiteSpace(duration)) return "—";
        if (string.Equals(unit, "Saat", StringComparison.OrdinalIgnoreCase) &&
            TimeSpan.TryParse(duration, CultureInfo.InvariantCulture, out var hours))
            duration = duration == "0" ? "00:00" : $"{(hours < TimeSpan.Zero ? "-" : "")}{Math.Abs((int)hours.TotalHours):00}:{hours.Duration().Minutes:00}";
        return $"{duration} {unit}";
    }

    public static string StatusText(Status? status) => status switch
    {
        Status.IslemBekliyor => "İşlem Bekliyor",
        Status.KontrolEdildi => "Kontrol Edildi",
        Status.KayitlaraIslendi => "Kayıtlara İşlendi",
        Status.Reddedildi => "Reddedildi",
        Status.IptalEdildi => "İptal Edildi",
        Status.DilekceBekleniyor => "Dilekçe Bekleniyor",
        _ => "Durum Belirsiz"
    };
}
