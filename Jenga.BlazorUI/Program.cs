using Jenga.BlazorUI.Components;
using Jenga.BlazorUI.Services.Menu;
using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories;
using Jenga.DataAccess.Repositories.IRepository;
using Jenga.DataAccess.Services.Inventory;
using Jenga.DataAccess.Services.Menu;
using Jenga.Utility.Error;
using Jenga.Utility.Logging;
using Jenga.Utility.Modal;
using Jenga.Utility.Toast;
using Microsoft.EntityFrameworkCore;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

var builder = WebApplication.CreateBuilder(args);
var logger = builder.Services.BuildServiceProvider()
    .GetRequiredService<ILogger<ApplicationDbContext>>();
// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
/*SB*/
//builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")),
//           ServiceLifetime.Transient);
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
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<IMenuItemService, MenuItemService>();
builder.Services.AddScoped<MenuStateService>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
// Toast Service
builder.Services.AddScoped<IToastService, ToastService>();
//Logging Services
builder.Services.AddScoped<ILogService, LogService>();
builder.Services.AddScoped<ILogWriter, FileLogWriter>();
//Error Handling 
builder.Services.AddScoped<IErrorService, ErrorService>();
//Modal Service
builder.Services.AddScoped<IModalService, ModalService>();
//Rol Service
builder.Services.AddScoped<IRolService, RolService>();
//inventory services
builder.Services.AddScoped<IMaterialService, MaterialService>();
builder.Services.AddScoped<IMaterialCategoryService, MaterialCategoryService>();
builder.Services.AddScoped<IMaterialBrandService, MaterialBrandService>();
builder.Services.AddScoped<IMaterialModelService, MaterialModelService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IMaterialEntryService, MaterialEntryService>();
builder.Services.AddScoped<IMaterialUnitService, MaterialUnitService>();

//DetailedErrors ayarını aç
builder.Services.AddServerSideBlazor()
    .AddCircuitOptions(options => { options.DetailedErrors = true; });

var app = builder.Build();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


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
