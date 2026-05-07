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
//   * Yoksa ve Negotiate Authorization header'ı varsa -> Negotiate scheme.
//   * Hiçbiri yoksa, challenge sırasında Cookie scheme (login formuna yönlendirir).
//     Böylece iPhone'da NTLM prompt'u oluşmaz.
const string PolicyScheme = "JengaAuth";

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

            // 3. Diğer durumlarda cookie (login formuna yönlendirir).
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
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder(
            AuthEndpoints.CookieScheme,
            NegotiateDefaults.AuthenticationScheme)
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
