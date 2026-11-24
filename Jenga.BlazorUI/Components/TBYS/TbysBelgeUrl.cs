namespace Jenga.BlazorUI.Components.TBYS;

internal static class TbysBelgeUrl
{
    private const string BaseUrl = "https://portal.tskgv.org.tr/YonetimBirimleri/InsaatVeEmlakYonetimiSubesi/TBYSBelgeleri";
    internal const string TasinmazRaporu = "TasinmazRaporu";

    internal static string Olustur(string dosyaOnEki, int tasinmazId)
        => FormattableString.Invariant($"{BaseUrl}/{dosyaOnEki}{tasinmazId}.pdf");
}
