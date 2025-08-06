using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository;
using Jenga.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Jenga.DataAccess.Repositories.IRepository.Menu;
using Jenga.DataAccess.Repositories.Menu;
using Jenga.BlazorWeb.Services.Theme;
using Jenga.BlazorWeb.Services.Menu;
using Jenga.BlazorWeb.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

/*<SB>*/

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<IRolService, RolService>();
builder.Services.AddScoped<IMenuItemRepository, MenuItemRepository>();
builder.Services.AddScoped<Func<IUnitOfWork>>(provider =>
{
    return () => provider.GetRequiredService<IUnitOfWork>();
});
//Dark-Light Theme Service
builder.Services.AddScoped<ThemeService>();


/*</SB>*/
var app = builder.Build();
/*<SB>*/

/*<SB>*/

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

app.Run();
