using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.IKYS;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Components.Authorization;

namespace Jenga.BlazorUI.Services.Common
{
    public class CurrentUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AuthenticationStateProvider _authStateProvider;
        private Personel? _cachedPersonel;

        public CurrentUserService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor, AuthenticationStateProvider authStateProvider)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
            _authStateProvider = authStateProvider;
        }

        public async Task<Personel?> GetCurrentPersonelAsync()
        {
            if (_cachedPersonel != null) return _cachedPersonel;

            // Try IHttpContextAccessor first (works for HTTP requests)
            var principal = _httpContextAccessor.HttpContext?.User;

            // If HttpContext is null (Blazor Server circuit), use AuthenticationStateProvider
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

            // Normalize: domain\user -> user, user@domain -> user (adjust to your conventions)
            if (userName.Contains('\\')) userName = userName.Split('\\', 2)[1];
            if (userName.Contains('@')) userName = userName.Split('@', 2)[0];

            var normalized = userName.Trim().ToLowerInvariant();

            // Find matching Personel by KullaniciAdi (case-insensitive)
            _cachedPersonel = await _unitOfWork.Personel.GetFirstOrDefaultAsync(
                p => p.KullaniciAdi != null && p.KullaniciAdi.ToLower() == normalized,
                includeProperties: null,
                trackChanges: false);

            return _cachedPersonel;
        }
    }
}