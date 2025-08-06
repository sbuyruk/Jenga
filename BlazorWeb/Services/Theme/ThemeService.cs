using Microsoft.JSInterop;

namespace Jenga.BlazorWeb.Services.Theme
{

    public class ThemeService
    {
        private readonly IJSRuntime _js;
        public string CurrentTheme { get; private set; } = "light";

        public ThemeService(IJSRuntime js)
        {
            _js = js;
        }

        public async Task Toggle()
        {
            CurrentTheme = CurrentTheme == "light" ? "dark" : "light";
            await _js.InvokeVoidAsync("setThemeMode", CurrentTheme);
            OnThemeChanged?.Invoke();
        }
        public event Action? OnThemeChanged;

    }

}
