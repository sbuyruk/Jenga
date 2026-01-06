using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.IKYS;
using Microsoft.AspNetCore.Components.Authorization;

namespace Jenga.BlazorUI.Services.Common
{
    public class CurrentUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly ImpersonationService _impersonationService;
        private Personel? _cachedPersonel;

        public CurrentUserService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor, AuthenticationStateProvider authStateProvider, ImpersonationService impersonationService)
        {
            _unitOfWork = unitOfWork;
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

            _cachedPersonel = await _unitOfWork.Personel.GetFirstOrDefaultAsync(
                p => p.KullaniciAdi != null && p.KullaniciAdi.ToLower() == normalizedDefault,
                includeProperties: null,
                trackChanges: false);

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
                _cachedPersonel = await _unitOfWork.Personel.GetFirstOrDefaultAsync(
                    p => p.KullaniciAdi != null && p.KullaniciAdi.ToLower() == normalized,
                    includeProperties: null,
                    trackChanges: false);

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