using Microsoft.JSInterop;

namespace Jenga.BlazorUI.Services.Common;

/// <summary>
/// Runtime tema yönetimi: aktif temayı tutar, değiştirir ve LocalStorage'a kaydeder.
/// </summary>
public class ThemeService
{
    private const string StorageKey = "jenga-theme";

    public static readonly IReadOnlyList<ThemeDefinition> Themes =
    [
        new("p1", "Kurumsal Lacivert & Gül"),
        new("p2", "Bordo & Krem"),
        new("p3", "Antrasit & Safran"),
        new("p4", "Lacivert & Altın"),
        new("p5", "Koyu Yeşil & Krem"),
        new("p6", "Petrol Mavisi & Bakır"),
    ];

    private readonly IJSRuntime _js;
    private string _current = "p1";

    public string Current => _current;

    public event Action? OnThemeChanged;

    public ThemeService(IJSRuntime js)
    {
        _js = js;
    }

    /// <summary>
    /// LocalStorage'dan kaydedilmiş temayı yükler ve HTML'e uygular.
    /// OnAfterRenderAsync(firstRender) içinden çağrılmalıdır.
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            var saved = await _js.InvokeAsync<string?>("themeStorage.get", StorageKey);
            if (!string.IsNullOrEmpty(saved) && Themes.Any(t => t.Key == saved))
                _current = saved;
        }
        catch { /* prerender veya JS henüz hazır değil */ }

        await ApplyAsync(_current);
    }

    /// <summary>Temayı değiştirir, DOM'a uygular ve LocalStorage'a kaydeder.</summary>
    public async Task SetThemeAsync(string themeKey)
    {
        if (_current == themeKey) return;
        if (!Themes.Any(t => t.Key == themeKey)) return;

        _current = themeKey;
        await ApplyAsync(themeKey);

        try { await _js.InvokeVoidAsync("themeStorage.set", StorageKey, themeKey); }
        catch { /* SSR/prerender */ }

        OnThemeChanged?.Invoke();
    }

    private async Task ApplyAsync(string themeKey)
    {
        try { await _js.InvokeVoidAsync("themeStorage.apply", themeKey); }
        catch { /* SSR/prerender */ }
    }
}

public record ThemeDefinition(string Key, string DisplayName);
