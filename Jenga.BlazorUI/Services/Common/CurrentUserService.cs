using Jenga.DataAccess.Data;
using Jenga.Models.IKYS;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jenga.BlazorUI.Services.Common
{
    public class CurrentUserService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly ImpersonationService _impersonationService;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly ILogger<CurrentUserService> _logger;
        private Personel? _cachedPersonel;

        public CurrentUserService(
            IDbContextFactory<ApplicationDbContext> dbFactory,
            IHttpContextAccessor httpContextAccessor,
            AuthenticationStateProvider authStateProvider,
            ImpersonationService impersonationService,
            IHostEnvironment hostEnvironment,
            ILogger<CurrentUserService> logger)
        {
            _dbFactory = dbFactory;
            _httpContextAccessor = httpContextAccessor;
            _authStateProvider = authStateProvider;
            _impersonationService = impersonationService;
            _hostEnvironment = hostEnvironment;
            _logger = logger;
        }

        /// <summary>
        /// Prerender / <see cref="Microsoft.AspNetCore.Components.ComponentBase.OnInitializedAsync"/> gibi
        /// JS interop'un henüz hazır olmadığı aşamalarda çağrılır.
        /// Impersonation override'ı atlar; JS bağlantısı kurulduktan sonra
        /// <see cref="GetCurrentPersonelAsync"/> ile tekrar çözümlenebilir.
        /// </summary>
        public Task<Personel?> GetCurrentPersonelWithoutImpersonationAsync()
            => GetCurrentPersonelAsync(skipImpersonation: true);

        public Task<Personel?> GetCurrentPersonelAsync()
            => GetCurrentPersonelAsync(skipImpersonation: false);

        private async Task<Personel?> GetCurrentPersonelAsync(bool skipImpersonation)
        {
            if (_cachedPersonel != null) return _cachedPersonel;

            // Development ortamında impersonation override kontrolü.
            // JS interop prerender sırasında çalışmayabileceğinden sessizce atlanır.
            if (!skipImpersonation && _hostEnvironment.IsDevelopment())
            {
                try
                {
                    var overrideUser = await _impersonationService.GetOverrideAsync();
                    if (!string.IsNullOrWhiteSpace(overrideUser))
                    {
                        await using var overrideDb = await _dbFactory.CreateDbContextAsync();
                        var overridePersonel = await overrideDb.Personel_Table.AsNoTracking()
                            .FirstOrDefaultAsync(p => p.KullaniciAdi != null &&
                                                      EF.Functions.Like(p.KullaniciAdi, overrideUser.Trim()));
                        if (overridePersonel != null)
                        {
                            _cachedPersonel = overridePersonel;
                            return _cachedPersonel;
                        }
                    }
                }
                catch
                {
                    // Prerender sırasında JS interop çalışmaz; hata normal, görmezden gelinir.
                }
            }

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
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "AuthenticationState çözülemedi.");
                    principal = null;
                }
            }

            // GÜVENLİK: Daha önce burada `Environment.UserName` fallback'i vardı.
            // Bu, Negotiate auth fail olduğunda sunucunun uygulama-pool/host hesabını
            // Personel olarak çözmeye çalışıyor ve DB'de denk düşen bir KullaniciAdi varsa
            // anonim ziyaretçinin o kişi gibi davranmasına yol açıyordu (auth-bypass).
            // Auth çözülemediyse null dönmek doğru davranış; üst katman [Authorize] uygular.
            var userName = principal?.Identity?.Name;
            if (string.IsNullOrWhiteSpace(userName))
            {
                _logger.LogDebug("GetCurrentPersonelAsync: kimlik çözümlenemedi.");
                return null;
            }

            // DOMAIN\\user veya user@domain biçimini normalize et.
            if (userName.Contains('\\')) userName = userName.Split('\\', 2)[1];
            if (userName.Contains('@')) userName = userName.Split('@', 2)[0];

            var trimmed = userName.Trim();
            if (string.IsNullOrEmpty(trimmed)) return null;

            // SQL Server'ın varsayılan CI collation'ı sayesinde Like exact match case-insensitive çalışır.
            await using var db = await _dbFactory.CreateDbContextAsync();
            var personel = await db.Personel_Table.AsNoTracking()
                .FirstOrDefaultAsync(p => p.KullaniciAdi != null &&
                                          EF.Functions.Like(p.KullaniciAdi, trimmed));

            // null olursa cache'leme; auth sonradan geldiğinde tekrar denesin.
            if (personel != null) _cachedPersonel = personel;
            return personel;
        }

        /// <summary>
        /// Önbelleği temizler; bir sonraki çağrıda kullanıcı DB'den yeniden çözümlenir.
        /// Impersonation değişikliği veya oturum güncellemesi sonrasında çağrılmalıdır.
        /// </summary>
        public void Invalidate() => _cachedPersonel = null;

        // Impersonation override (Development-only; prod'da çağrı bile başarısız döner).
        // Üst katman da ayrıca [Authorize] / rol kontrolü yapmalıdır.
        public async Task<bool> SetImpersonationOverrideAsync(string? overrideUser)
        {
            if (!_hostEnvironment.IsDevelopment())
            {
                _logger.LogWarning("Impersonation girişimi engellendi: Development dışı ortam.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(overrideUser))
            {
                Invalidate();
                return false;
            }

            var trimmed = overrideUser.Trim();

            // Yeni kullanıcıya geçmeden önce mevcut cache'i temizle.
            Invalidate();

            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync();
                var found = await db.Personel_Table.AsNoTracking()
                    .FirstOrDefaultAsync(p => p.KullaniciAdi != null &&
                                              EF.Functions.Like(p.KullaniciAdi, trimmed));

                if (found != null)
                {
                    await _impersonationService.SetOverrideAsync(trimmed);
                    _cachedPersonel = found;
                    return true;
                }

                _logger.LogWarning("SetImpersonationOverrideAsync: '{User}' bulunamadı.", trimmed);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SetImpersonationOverrideAsync başarısız.");
                return false;
            }
        }
    }
}