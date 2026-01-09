using Jenga.BlazorUI.Components;
using Jenga.BlazorUI.Services.Common;
using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories;
using Jenga.DataAccess.Repositories.IRepository;
using Jenga.DataAccess.Services.Common;
using Jenga.DataAccess.Services.IKYS;
using Jenga.DataAccess.Services.Inventory;
using Jenga.DataAccess.Services.TBYS;
using Jenga.Utility.Error;
using Jenga.Utility.Logging;
using Jenga.Utility.Toast;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.EntityFrameworkCore;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using System.Globalization;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);
var logger = builder.Services.BuildServiceProvider()
    .GetRequiredService<ILogger<ApplicationDbContext>>();
// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
/*SB*/
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.EnableSensitiveDataLogging();

    options.LogTo(
        message => logger.LogInformation(message), // EF logları buraya akar
        new[] { DbLoggerCategory.Database.Command.Name },
        LogLevel.Information
    );
}, ServiceLifetime.Transient);

/*SB UnitOfWork */
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

/*SB Menu Servisi*/
builder.Services.AddScoped<IMenuItemService, MenuItemService>();
builder.Services.AddScoped<MenuStateService>();

// Toast Service
builder.Services.AddScoped<IToastService, ToastService>();
//Logging Services
builder.Services.AddScoped<ILogService, LogService>();
builder.Services.AddScoped<ILogWriter, FileLogWriter>();
//Error Handling 
builder.Services.AddScoped<IErrorService, ErrorService>();
//Modal Service
builder.Services.AddScoped<IModalService, ModalService>();
//Role Service
builder.Services.AddScoped<IRoleService, RoleService>();
//inventory services
builder.Services.AddScoped<IMaterialService, MaterialService>();
builder.Services.AddScoped<IMaterialCategoryService, MaterialCategoryService>();
builder.Services.AddScoped<IMaterialBrandService, MaterialBrandService>();
builder.Services.AddScoped<IMaterialModelService, MaterialModelService>();
builder.Services.AddScoped<IMaterialEntryService, MaterialEntryService>();
builder.Services.AddScoped<IMaterialUnitService, MaterialUnitService>();
builder.Services.AddScoped<IMaterialInventoryService, MaterialInventoryService>();
builder.Services.AddScoped<IMaterialMovementService, MaterialMovementService>();
builder.Services.AddScoped<IMaterialExitService, MaterialExitService>();
builder.Services.AddScoped<IMaterialTransferService, MaterialTransferService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IPersonelLocationService, PersonelLocationService>();
//TBYS Services
builder.Services.AddScoped<ITasinmazService, TasinmazService>();
builder.Services.AddScoped<ITasinmazBagisciService, TasinmazBagisciService>();
builder.Services.AddScoped<IKiraciService, KiraciService>();
builder.Services.AddScoped<IKiraSozlesmeService, KiraSozlesmeService>();
builder.Services.AddScoped<ISozlesmeTasinmazService, SozlesmeTasinmazService>();
builder.Services.AddScoped<IOdemePlaniService, OdemePlaniService>();
//Common Services
builder.Services.AddScoped<IBagisService, BagisService>();
builder.Services.AddScoped<IIlService, IlService>();
builder.Services.AddScoped<IBolgeService, BolgeService>();
builder.Services.AddScoped<CurrentUserService>();
builder.Services.AddScoped<ImpersonationService>();
//IKYS Service 
builder.Services.AddScoped<IPersonelService, PersonelService>();

//Currentusername alırken httpContextAcces.. kullanmak için
builder.Services.AddHttpContextAccessor();

// Authentication/Authorization for Windows auth (Negotiate)
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate();
builder.Services.AddAuthorization();

//DetailedErrors ayarını aç
builder.Services.AddServerSideBlazor()
    .AddCircuitOptions(options => { options.DetailedErrors = true; });

// Set default culture early so all code (including components) uses tr-TR by default
var defaultCulture = new CultureInfo("tr-TR");
CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;

builder.Services.AddLocalization(); // optional but recommended for localization support

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
