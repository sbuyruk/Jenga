using Jenga.BlazorUI.Components;
using Jenga.BlazorUI.Endpoints;
using Jenga.BlazorUI.Extensions;
using Jenga.BlazorUI.Services.Common.Auth;
using Jenga.DataAccess.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics;
using System.Globalization;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

// Data Protection key'lerini kalıcı dizine kaydet.
// Ephemeral (in-memory) key kullanılırsa App Pool her başladığında cookie'ler geçersiz kalır.
var dpKeyPath = Path.Combine(builder.Environment.ContentRootPath, "dp-keys");
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dpKeyPath))
    .SetApplicationName("JengaBlazorUI");

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Infrastructure (DbContext, DbContextScopeFactory)
builder.Services.AddInfrastructureServices(builder.Configuration, builder.Environment);

// DataAccess servisleri
builder.Services.AddCommonDataServices();
builder.Services.AddInventoryServices();
builder.Services.AddTbysServices();
builder.Services.AddIkysServices();
builder.Services.AddNbysServices();
builder.Services.AddFtkServices();
builder.Services.AddSearchServices();

// UI / uygulama servisleri
builder.Services.AddApplicationServices();
builder.Services.AddPresenceServices();

builder.Services.AddHttpContextAccessor();

// LDAP/AD doğrulayıcı (cookie login akışı için)
if (OperatingSystem.IsWindows())
{
    builder.Services.AddSingleton<LdapAuthenticator>();
}

// HİBRİT KİMLİK DOĞRULAMA:
//  - Intranet (Windows bilgisayarlar, IIS Negotiate çözüyor)  -> Windows Auth, kullanıcı şifre girmez.
//  - Internet (iPhone/VPN, Negotiate çalışmaz)                -> Cookie + AD form login.
//
// Strateji: PolicyScheme forwarder. Her istekte:
//   * Geçerli cookie varsa  -> Cookie scheme.
//   * Negotiate/NTLM header varsa -> Negotiate scheme.
//   * İntranet IP aralığındaysa -> Negotiate scheme (tarayıcı challenge'a otomatik cevap verir).
//   * Diğer durumlarda -> Cookie scheme (login formuna yönlendirir).
const string PolicyScheme = "JengaAuth";

// İntranet IP aralıkları — Negotiate challenge gönderilecek ağlar.
static bool IsIntranetRequest(Microsoft.AspNetCore.Http.HttpContext ctx)
{
    var ip = ctx.Connection.RemoteIpAddress;
    if (ip == null) return false;
    // IPv4-mapped IPv6 → IPv4'e çevir (::ffff:10.x.x.x gibi).
    if (ip.IsIPv4MappedToIPv6) ip = ip.MapToIPv4();
    if (ip.ToString() == "127.0.0.1" || ip.ToString() == "::1") return true;

    var bytes = ip.GetAddressBytes();
    if (bytes.Length != 4) return false;
    // 10.0.0.0/8
    if (bytes[0] == 10) return true;
    // 172.16.0.0/12
    if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) return true;
    // 192.168.0.0/16
    if (bytes[0] == 192 && bytes[1] == 168) return true;
    return false;
}

// Mobil/tablet User-Agent tespiti — VPN üzerinden gelen mobil cihazlar
// intranet IP alsa da Negotiate yerine Cookie (login formu) kullanmalıdır.
static bool IsMobileBrowser(Microsoft.AspNetCore.Http.HttpContext ctx)
{
    var ua = ctx.Request.Headers.UserAgent.ToString();
    if (string.IsNullOrEmpty(ua)) return false;
    return ua.Contains("Mobile",  StringComparison.OrdinalIgnoreCase)
        || ua.Contains("Android", StringComparison.OrdinalIgnoreCase)
        || ua.Contains("iPhone",  StringComparison.OrdinalIgnoreCase)
        || ua.Contains("iPad",    StringComparison.OrdinalIgnoreCase)
        || ua.Contains("Tablet",  StringComparison.OrdinalIgnoreCase);
}

builder.Services
    .AddAuthentication(PolicyScheme)
    .AddPolicyScheme(PolicyScheme, "Jenga Hibrit (Cookie + Negotiate)", o =>
    {
        o.ForwardDefaultSelector = ctx =>
        {
            // 1. Cookie varsa her zaman cookie scheme.
            if (ctx.Request.Cookies.ContainsKey(".Jenga.Auth"))
                return AuthEndpoints.CookieScheme;

            // 2. Tarayıcı Negotiate/NTLM header'ı gönderdiyse Windows Auth.
            var auth = ctx.Request.Headers.Authorization.ToString();
            if (!string.IsNullOrEmpty(auth) &&
                (auth.StartsWith("Negotiate", StringComparison.OrdinalIgnoreCase) ||
                 auth.StartsWith("NTLM", StringComparison.OrdinalIgnoreCase)))
                return NegotiateDefaults.AuthenticationScheme;

            // 3. İntranet IP'den geliyorsa ve mobil cihaz değilse Negotiate challenge gönder.
            //    Windows tarayıcısı (Edge/Chrome) challenge'a otomatik Kerberos/NTLM ile cevap verir.
            //    Mobil cihazlar VPN üzerinden intranet IP alsa da Negotiate desteklemez → Cookie.
            if (IsIntranetRequest(ctx) && !IsMobileBrowser(ctx))
                return NegotiateDefaults.AuthenticationScheme;

            // 4. Diğer durumlarda cookie (login formuna yönlendirir).
            return AuthEndpoints.CookieScheme;
        };
    })
    .AddCookie(AuthEndpoints.CookieScheme, o =>
    {
        o.Cookie.Name = ".Jenga.Auth";
        o.Cookie.HttpOnly = true;
        o.Cookie.SameSite = SameSiteMode.Lax;
        o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        o.LoginPath = AuthEndpoints.LoginPath;
        o.LogoutPath = AuthEndpoints.LogoutPath;
        o.AccessDeniedPath = AuthEndpoints.LoginPath;
        o.ExpireTimeSpan = TimeSpan.FromDays(30);
        o.SlidingExpiration = true;
        o.Cookie.MaxAge = TimeSpan.FromDays(30); // Tarayıcı kapansa bile cookie kalıcı olur (iOS Safari)
    })
    .AddNegotiate();

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder(PolicyScheme)
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddServerSideBlazor()
    .AddCircuitOptions(options => { options.DetailedErrors = true; });

var defaultCulture = new CultureInfo("tr-TR");
CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;

builder.Services.AddLocalization();
var app = builder.Build();

var supportedCultures = new[] { defaultCulture };
var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(defaultCulture),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
};

app.UseRequestLocalization(localizationOptions);

// Configure the HTTP request pipeline.
// NOT: UseHsts ve UseHttpsRedirection kasıtlı olarak kaldırıldı.
// HTTP→HTTPS yönlendirmesi IIS URL Rewrite modülü üzerinden yapılmalıdır.
// UseHsts tarayıcıda kalıcı HSTS kaydı bırakır; HTTP intranet bağlantısını kırar.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
else
{
    app.UseExceptionHandler(new ExceptionHandlerOptions
    {
        ExceptionHandlingPath = "/Error",
        AllowStatusCode404Response = true
    });
    app.UseDeveloperExceptionPage();
}

// Authentication/Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapAuthEndpoints();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
/*SB Hata Sayfası*/
app.MapFallback(context =>
{
    context.Response.Redirect($"/error?url={Uri.EscapeDataString(context.Request.Path)}");
    return Task.CompletedTask;
});

app.Run();
