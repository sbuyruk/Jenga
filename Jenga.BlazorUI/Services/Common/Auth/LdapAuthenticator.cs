using System.DirectoryServices.AccountManagement;
using System.Runtime.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Jenga.BlazorUI.Services.Common.Auth
{
    /// <summary>
    /// Active Directory'ye karşı kullanıcı adı/parola doğrulaması yapar.
    /// Cookie tabanlı login akışı için kullanılır (internet binding'i).
    /// Sunucunun Windows üzerinde, AD'ye erişebilen bir ortamda çalışması beklenir.
    /// </summary>
    [SupportedOSPlatform("windows")]
    public sealed class LdapAuthenticator
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<LdapAuthenticator> _logger;

        public LdapAuthenticator(IConfiguration configuration, ILogger<LdapAuthenticator> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public bool ValidateCredentials(
            string userName,
            string password,
            out string? samAccountName,
            out string? displayName)
        {
            samAccountName = null;
            displayName = null;

            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
                return false;

            // DOMAIN\user veya user@domain biçimini normalize et (yalnız user kalsın).
            var u = userName.Trim();
            if (u.Contains('\\')) u = u.Split('\\', 2)[1];
            if (u.Contains('@')) u = u.Split('@', 2)[0];
            if (string.IsNullOrEmpty(u)) return false;

            var domain         = _configuration["Auth:Ldap:Domain"];         // FQDN: tskgv.org.tr
            var domainFallback = _configuration["Auth:Ldap:DomainFallback"]; // NetBIOS: TSKGV
            var container      = _configuration["Auth:Ldap:Container"];       // null → tüm domain

            try
            {
                return TryValidate(u, password, domain, container, out samAccountName, out displayName)
                    || (!string.IsNullOrWhiteSpace(domainFallback)
                        && TryValidate(u, password, domainFallback, container, out samAccountName, out displayName));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LDAP doğrulama hatası: {User}", u);
                return false;
            }
        }

        private bool TryValidate(
            string u, string password,
            string? domain, string? container,
            out string? samAccountName, out string? displayName)
        {
            samAccountName = null;
            displayName    = null;

            try
            {
                var domainArg = string.IsNullOrWhiteSpace(domain) ? null : domain.Trim();

                // PrincipalContext oluşturma AD'ye ağ bağlantısı açar; VPN üzerinden
                // gecikebilir veya hiç dönmeyebilir. Task.Run ile 10 sn timeout uygula.
                string? samResult = null, displayResult = null;
                bool validated = false;

                var task = Task.Run(() =>
                {
                    using var ctx = string.IsNullOrWhiteSpace(container)
                        ? new PrincipalContext(ContextType.Domain, domainArg)
                        : new PrincipalContext(ContextType.Domain, domainArg, container.Trim());

                    // SimpleBind için DOMAIN\user formatı gerekiyor.
                    // Sadece kullanıcı adı gönderilirse AD reddeder.
                    var bindUser = string.IsNullOrWhiteSpace(domainArg)
                        ? u
                        : domainArg.Contains('.') 
                            ? $"{u}@{domainArg}"      // FQDN: user@tskgv.local
                            : $"{domainArg}\\{u}";    // NetBIOS: TSKGV\user

                    if (!ctx.ValidateCredentials(bindUser, password, ContextOptions.SimpleBind))
                    {
                        _logger.LogInformation("AD doğrulaması başarısız [{Domain}]: {User}", domainArg, bindUser);
                        return;
                    }

                    validated = true;
                    try
                    {
                        using var principal = UserPrincipal.FindByIdentity(ctx, u);
                        samResult     = principal?.SamAccountName ?? u;
                        displayResult = principal?.DisplayName ?? principal?.Name ?? samResult;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "AD kullanıcı bilgileri alınamadı [{Domain}]: {User}. Giriş kabul edildi.", domainArg, u);
                        samResult     = u;
                        displayResult = u;
                    }

                    _logger.LogInformation("AD doğrulaması başarılı [{Domain}]: {User} ({Display})", domainArg, samResult, displayResult);
                });

                if (!task.Wait(TimeSpan.FromSeconds(10)))
                {
                    _logger.LogWarning("TryValidate zaman aşımı [{Domain}]: {User}", domainArg, u);
                    return false;
                }

                // task içinde exception fırlattıysa yüzeye çıkar
                task.GetAwaiter().GetResult();

                samAccountName = samResult;
                displayName    = displayResult;
                return validated;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "TryValidate başarısız [{Domain}]: {User}", domain, u);
                return false;
            }
        }
    }
}
