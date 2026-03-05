using LinkUp.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace LinkUp.Web.Middleware;

// Middleware que verifica que el usuario autenticado esté activo.
// Si un usuario fue desactivado se cierra su sesión.
public class ActiveUserMiddleware
{
    private readonly RequestDelegate _next;

    public ActiveUserMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context,
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = userManager.GetUserId(context.User);
            if (userId != null)
            {
                var user = await userManager.FindByIdAsync(userId);
                if (user == null || !user.IsActive)
                {
                    await signInManager.SignOutAsync();
                    context.Response.Redirect("/Account/Login?message=Tu+cuenta+ha+sido+desactivada");
                    return;
                }
            }
        }

        await _next(context);
    }
}
