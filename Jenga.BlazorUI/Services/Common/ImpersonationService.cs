using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Jenga.BlazorUI.Services.Common
{
    public class ImpersonationService
    {
        private readonly IJSRuntime _js;
        private const string Key = "jenga:impersonateUser";

        public ImpersonationService(IJSRuntime js)
        {
            _js = js;
        }

        public ValueTask SetOverrideAsync(string username)
            => _js.InvokeVoidAsync("sessionStorage.setItem", Key, username);

        public async ValueTask<string?> GetOverrideAsync()
        {
            try
            {
                return await _js.InvokeAsync<string?>("sessionStorage.getItem", Key);
            }
            catch (InvalidOperationException)
            {
                return null; // JS interop not available during prerender
            }
            catch (JSDisconnectedException)
            {
                return null;
            }
        }

        public ValueTask ClearOverrideAsync()
            => _js.InvokeVoidAsync("sessionStorage.removeItem", Key);
    }
}