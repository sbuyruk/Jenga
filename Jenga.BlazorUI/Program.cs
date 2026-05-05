using Jenga.BlazorUI.Components;
using Jenga.BlazorUI.Extensions;
using Jenga.DataAccess.Extensions;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Diagnostics;
using System.Globalization;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate();
builder.Services.AddAuthorization(options =>
{
    // Tüm endpoint'lerde Windows Auth (Negotiate) challenge'ı zorla.
    // Bu olmadan Kestrel anonim istekleri geçirir; HttpContext.User.Identity.IsAuthenticated = false kalır.
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

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
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
else
{
    // Development'ta da yakalanmayan istisnaları logla; ardından developer page'e izin ver.
    app.UseExceptionHandler(new ExceptionHandlerOptions
    {
        ExceptionHandlingPath = "/Error",
        AllowStatusCode404Response = true
    });
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

// Authentication/Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
/*SB Hata Sayfası*/
app.MapFallback(context =>
{
    context.Response.Redirect($"/error?url={Uri.EscapeDataString(context.Request.Path)}");
    return Task.CompletedTask;
});

app.Run();
