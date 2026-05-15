using System.Runtime.Versioning;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Jenga.BlazorUI.Services.Common.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Jenga.BlazorUI.Endpoints
{
    public static class AuthEndpoints
    {
        public const string CookieScheme = "JengaCookies";
        public const string LoginPath    = "/account/login";
        public const string LogoutPath   = "/account/logout";
        public const string DiagPath     = "/auth/diag";

        public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet(DiagPath, (HttpContext ctx) =>
            {
                var req       = ctx.Request;
                var hasCookie = req.Cookies.ContainsKey(".Jenga.Auth");
                var authHdr   = req.Headers.Authorization.ToString();
                var user      = ctx.User.Identity?.Name ?? "(yok)";
                var isAuth    = ctx.User.Identity?.IsAuthenticated ?? false;
                var scheme    = ctx.User.Identity?.AuthenticationType ?? "(yok)";
                var ua        = req.Headers.UserAgent.ToString();
                var isHttps   = req.IsHttps;
                var host      = req.Host.ToString();
                var cookies   = string.Join(", ", req.Cookies.Keys);
                var enc       = HtmlEncoder.Default;

                var warn1  = !isHttps
                    ? "<li class='text-danger'>HTTPS degil - SecurePolicy=Always oldugundan cookie SET EDILMEZ.</li>"
                    : string.Empty;
                var warn3  = hasCookie && !isAuth
                    ? "<li class='text-danger'>Cookie var ama kimlik cozulemedi - suresi dolmus veya key degisti.</li>"
                    : string.Empty;
                var ok1    = isAuth ? "<li class='text-success'>Kimlik dogrulandi.</li>" : string.Empty;
                var hdrNote = !string.IsNullOrEmpty(authHdr)
                    ? "<li class='text-danger'>Authorization header mevcut - IIS Anonymous Authentication KAPALI. IIS Manager acan etkinlestirin.</li>"
                    : "<li class='text-success'>Authorization header yok - IIS Anonymous Authentication acik (dogru).</li>";
                var winNote = OperatingSystem.IsWindows()
                    ? "<li class='text-success'>Sunucu Windows ortaminda.</li>"
                    : "<li class='text-danger'>Sunucu Windows degil - AD dogrulamasi calismiyor.</li>";

                var html = $"<!DOCTYPE html><html lang='tr'><head><meta charset='utf-8'/><meta name='viewport' content='width=device-width,initial-scale=1'/><title>Auth Tani</title><link rel='stylesheet' href='/lib/bootstrap/dist/css/bootstrap.min.css'/></head><body class='p-3'><h4>Auth Tani</h4><table class='table table-bordered table-sm small'><tr><th>HTTPS mi?</th><td>{isHttps}</td></tr><tr><th>Host</th><td>{enc.Encode(host)}</td></tr><tr><th>Kimlik dogrulandi mi?</th><td>{isAuth}</td></tr><tr><th>Kullanici</th><td>{enc.Encode(user)}</td></tr><tr><th>Auth scheme</th><td>{enc.Encode(scheme)}</td></tr><tr><th>.Jenga.Auth cookie?</th><td>{hasCookie}</td></tr><tr><th>Tum cookieler</th><td>{enc.Encode(string.IsNullOrEmpty(cookies) ? "(hic yok)" : cookies)}</td></tr><tr><th>Authorization header</th><td>{enc.Encode(string.IsNullOrEmpty(authHdr) ? "(yok)" : authHdr)}</td></tr><tr><th>User-Agent</th><td style='word-break:break-all'>{enc.Encode(ua)}</td></tr><tr><th>Windows ortami?</th><td>{OperatingSystem.IsWindows()}</td></tr></table><h5>Yorumlar</h5><ul>{warn1}{warn3}{ok1}{hdrNote}{winNote}</ul><a href='/account/login' class='btn btn-primary btn-sm'>Login Sayfasi</a><a href='/' class='btn btn-secondary btn-sm ms-2'>Ana Sayfa</a></body></html>";
                return Results.Content(html, "text/html");
            }).AllowAnonymous();

            app.MapGet(LoginPath, (string? returnUrl, string? error) =>
                Results.Content(BuildLoginHtml(returnUrl, error), "text/html")
            ).AllowAnonymous();

            app.MapPost(LoginPath, async (HttpContext ctx, LdapAuthenticator? ldap) =>
            {
                if (!OperatingSystem.IsWindows())
                    return Results.Content(BuildLoginHtml(null, "notwindows"), "text/html");

                if (ldap is null)
                    return Results.Content(BuildLoginHtml(null, "noservice"), "text/html");

                var form      = await ctx.Request.ReadFormAsync();
                var userName  = form["username"].ToString().Trim();
                var password  = form["password"].ToString();
                var returnUrl = form["returnUrl"].ToString();

                if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
                    return RedirectToLogin(returnUrl, "empty");

                bool ok;
                string? sam, displayName;
                try
                {
                    // AD bağlantısı askıda kalmasın diye 12 saniyelik timeout.
                    string? samCapture = null, displayCapture = null;
                    bool okCapture = false;
                    var adTask = Task.Run(() =>
                    {
                        okCapture = ldap.ValidateCredentials(userName, password, out samCapture, out displayCapture);
                    });
                    if (!adTask.Wait(TimeSpan.FromSeconds(12)))
                    {
                        return RedirectToLogin(returnUrl, "adtimeout");
                    }
                    await adTask; // exception varsa yüzeye çıkar
                    ok = okCapture;
                    sam = samCapture;
                    displayName = displayCapture;
                }
                catch
                {
                    return RedirectToLogin(returnUrl, "adconnect");
                }

                if (!ok)
                    return RedirectToLogin(returnUrl, "wrongpass");

                var claims = new List<Claim>
                {
                    new(ClaimTypes.Name,      sam         ?? userName),
                    new(ClaimTypes.GivenName, displayName ?? sam ?? userName),
                    new("auth_source",        "ldap")
                };
                var identity  = new ClaimsIdentity(claims, CookieScheme);
                var principal = new ClaimsPrincipal(identity);

                await ctx.SignInAsync(CookieScheme, principal, new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc   = DateTimeOffset.UtcNow.AddHours(8),
                    AllowRefresh = true
                });

                var safeReturn = IsLocalUrl(returnUrl) ? returnUrl : "/";
                return Results.Redirect(safeReturn!);
            }).AllowAnonymous().DisableAntiforgery();

            app.MapGet(LogoutPath, async (HttpContext ctx) =>
            {
                await ctx.SignOutAsync(CookieScheme);
                return Results.Redirect("/");
            }).AllowAnonymous();

            return app;
        }

        private static IResult RedirectToLogin(string returnUrl, string errorCode)
        {
            var url = $"{LoginPath}?error={Uri.EscapeDataString(errorCode)}";
            if (!string.IsNullOrEmpty(returnUrl))
                url += "&returnUrl=" + Uri.EscapeDataString(returnUrl);
            return Results.Redirect(url);
        }

        private static bool IsLocalUrl(string? url)
            => !string.IsNullOrEmpty(url)
               && url.StartsWith('/')
               && !url.StartsWith("//",  StringComparison.Ordinal)
               && !url.StartsWith("/\\", StringComparison.Ordinal);

        private static string BuildLoginHtml(string? returnUrl, string? error)
        {
            var enc = HtmlEncoder.Default;
            var ret = enc.Encode(returnUrl ?? "/");

            var errMsg = error switch
            {
                "wrongpass"  => "Kullanici adi veya parola hatali.",
                "empty"      => "Kullanici adi ve parola bos birakilamaz.",
                "adconnect"  => "Active Directory baglantisindan kurulamadi. Sistem yoneticinize basvurun.",
                "adtimeout"  => "Active Directory sunucusuna baglanti zaman asimina ugradi. VPN baglantinizi kontrol edin.",
                "notwindows" => "Sunucu Windows uzerinde calismiyor.",
                "noservice"  => "Kimlik dogrulama servisi baslatılamadi.",
                null or ""   => string.Empty,
                _            => $"Giris basarisiz (kod: {enc.Encode(error)})."
            };
            var errBlock = string.IsNullOrEmpty(errMsg)
                ? string.Empty
                : $"<div class='alert alert-danger'>{errMsg}</div>";

            return $@"<!DOCTYPE html>
<html lang='tr'>
<head>
    <meta charset='utf-8' />
    <meta name='viewport' content='width=device-width, initial-scale=1.0' />
    <title>Giriş - Jenga</title>
    <link rel='stylesheet' href='https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css' integrity='sha384-QWTKZyjpPEjISv5WaRU9OFeRpok6YctnYmDr5pNlyT2bRjXh0JMhjY6hW+ALEwIH' crossorigin='anonymous' />
    <style>
        body {{ background:#f5f7fb; }}
        .login-card {{ max-width:380px; margin:8vh auto; }}
    </style>
</head>
<body>
    <div class='login-card card shadow-sm'>
        <div class='card-body p-4'>
            <h5 class='text-center mb-4'>🔐 Jenga Giriş</h5>
            {errBlock}
            <form method='post' action='{LoginPath}' autocomplete='on'>
                <input type='hidden' name='returnUrl' value='{ret}' />
                <div class='mb-3'>
                    <label class='form-label'>Kullanıcı Adı</label>
                    <input type='text' name='username' class='form-control'
                           autocomplete='username' required autofocus />
                    <div class='form-text text-muted'>Örn: ademir</div>
                </div>
                <div class='mb-3'>
                    <label class='form-label'>Parola</label>
                    <input type='password' name='password' class='form-control'
                           autocomplete='current-password' required />
                </div>
                <button type='submit' class='btn btn-primary w-100'
                        onclick='this.disabled=true;this.innerText=""Doğrulanıyor..."";this.form.submit()'>Giriş Yap</button>
            </form>
            <div class='text-center text-muted small mt-3'>Kurum (AD) hesabınızla giriş yapın.</div>
        </div>
    </div>
</body>
</html>";
        }
    }
}