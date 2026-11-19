using System.Globalization;
using System.Text.RegularExpressions;

namespace Jenga.Models.TBYS.Kontrol;

public static class KontrolKoordinat
{
    public static bool TryParse(string? deger, out decimal enlem, out decimal boylam)
    {
        enlem = boylam = 0;
        if (string.IsNullOrWhiteSpace(deger) || deger.Length > 100) return false;
        var eslesme = Regex.Match(deger,
            @"^\s*([+-]?\d+(?:\.\d+)?)(?:\s*[,;]\s*|\s+)([+-]?\d+(?:\.\d+)?)\s*$");
        return eslesme.Success
            && decimal.TryParse(eslesme.Groups[1].Value, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out enlem)
            && decimal.TryParse(eslesme.Groups[2].Value, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out boylam)
            && enlem is >= -90 and <= 90 && boylam is >= -180 and <= 180;
    }

    public static string? GoogleHaritaAdresi(string? koordinat)
    {
        if (!TryParse(koordinat, out var enlem, out var boylam)) return null;
        var sorgu = $"{enlem.ToString(CultureInfo.InvariantCulture)},{boylam.ToString(CultureInfo.InvariantCulture)}";
        return $"https://www.google.com/maps/search/?api=1&query={Uri.EscapeDataString(sorgu)}";
    }
}
