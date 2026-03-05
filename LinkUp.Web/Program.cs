using LinkUp.Application.Common;
using LinkUp.Domain.Entities;
using LinkUp.Infrastructure.DependencyInjection;
using LinkUp.Infrastructure.Persistence;
using LinkUp.Web.Filters;
using LinkUp.Web.Middleware;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Registrar el filtro como servicio (necesario para DI)
builder.Services.AddScoped<PendingRequestsFilter>();

builder.Services.AddControllersWithViews(options =>
{
    // Filtro global para inyectar contador de solicitudes pendientes en el navbar
    options.Filters.Add<PendingRequestsFilter>();
});

builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ReturnUrlParameter = "returnUrl";
    // Redirigir con mensaje al intentar acceder a ruta protegida sin sesión
    options.Events.OnRedirectToLogin = context =>
    {
        var path = context.Request.Path;
        context.Response.Redirect(
            $"/Account/Login?returnUrl={Uri.EscapeDataString(path)}" +
            "&message=Debe+iniciar+sesi%C3%B3n+para+acceder+a+esta+secci%C3%B3n.");
        return Task.CompletedTask;
    };
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// Middleware para verificar que usuarios autenticados sigan activos
app.UseMiddleware<ActiveUserMiddleware>();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
    await context.Database.MigrateAsync();
    await DataSeeder.SeedAsync(context, userManager);
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
