using System.Globalization;

namespace Jenga.Models.IKYS;

public static class PersonnelInstitution
{
    public static string Normalize(string? value)
        => string.Join(" ", (value ?? "").Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    public static string Key(string? value) => Normalize(value).ToUpper(CultureInfo.GetCultureInfo("tr-TR"));
}
