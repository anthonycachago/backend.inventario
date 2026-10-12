using System.Globalization;
using Inventario.Web.Components;
using Inventario.Web.Domain;
using Inventario.Web.Services;
using Microsoft.EntityFrameworkCore;

// Formato de números y fechas en español (Ecuador): 1.234,50 y dd/MM/yyyy
var cultura = new CultureInfo("es-EC");
CultureInfo.DefaultThreadCurrentCulture = cultura;
CultureInfo.DefaultThreadCurrentUICulture = cultura;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Blazor Server: usar la factory y crear un contexto por operación (no inyectar AppDbContext directo en componentes)
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<InventarioService>();

var app = builder.Build();

// En desarrollo aplica migraciones pendientes; en producción usar los scripts de /Scripts.
// En ambos casos garantiza que exista el usuario técnico al que se atribuyen los movimientos.
await DbInicializador.InicializarAsync(app.Services, aplicarMigraciones: app.Environment.IsDevelopment());

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
