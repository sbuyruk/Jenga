using BlazorWeb.Components;
using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository;
using Jenga.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using Jenga.DataAccess.Repositories.IRepository.Menu;
using Jenga.DataAccess.Repositories.Menu;
using Jenga.BlazorUI.Services.Menu;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

/*<SB>*/

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<IMenuItemRepository, MenuItemRepository>();




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
