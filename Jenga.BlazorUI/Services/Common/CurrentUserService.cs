using Jenga.DataAccess.Data;
using Jenga.Models.IKYS;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Jenga.BlazorUI.Services.Common
{
    public class CurrentUserService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly ImpersonationService _impersonationService;
        private Personel? _cachedPersonel;

        public CurrentUserService(IDbContextFactory<ApplicationDbContext> dbFactory, IHttpContextAccessor httpContextAccessor, AuthenticationStateProvider authStateProvider, ImpersonationService impersonationService)
        {
            _dbFactory = dbFactory;
            _httpContextAccessor = httpContextAccessor;
            _authStateProvider = authStateProvider;
            _impersonationService = impersonationService;
        }

        public async Task<Personel?> GetCurrentPersonelAsync()
        {
            if (_cachedPersonel != null) return _cachedPersonel;

            // NOTE: Do NOT call JS interop from here (prerender may be active).
            // Resolve from HttpContext or AuthenticationState instead.
            var principal = _httpContextAccessor.HttpContext?.User;

            if (principal == null || principal.Identity == null || !principal.Identity.IsAuthenticated)
            {
                try
                {
                    var authState = await _authStateProvider.GetAuthenticationStateAsync();
                    principal = authState?.User;
                }
                catch
                {
                    principal = null;
                }
            }

            var httpUserName = principal?.Identity?.Name;
            var userName = string.IsNullOrWhiteSpace(httpUserName) ? Environment.UserName : httpUserName;

            if (userName.Contains('\\')) userName = userName.Split('\\', 2)[1];
            if (userName.Contains('@')) userName = userName.Split('@', 2)[0];

            var normalizedDefault = userName.Trim().ToLowerInvariant();

            await using var db = await _dbFactory.CreateDbContextAsync();
            _cachedPersonel = await db.Personel_Table.AsNoTracking()
                .FirstOrDefaultAsync(p => p.KullaniciAdi != null && p.KullaniciAdi.ToLower() == normalizedDefault);

            return _cachedPersonel;
        }

        // New: apply an impersonation override value coming from client JS (sessionStorage)
        public async Task<bool> SetImpersonationOverrideAsync(string? overrideUser)
        {
            if (string.IsNullOrWhiteSpace(overrideUser))
                return false;

            var normalized = overrideUser.Trim().ToLowerInvariant();

            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync();
                _cachedPersonel = await db.Personel_Table.AsNoTracking()
                    .FirstOrDefaultAsync(p => p.KullaniciAdi != null && p.KullaniciAdi.ToLower() == normalized);

                return _cachedPersonel != null;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"SetImpersonationOverrideAsync failed: {ex}");
                return false;
            }
        }
    }
}