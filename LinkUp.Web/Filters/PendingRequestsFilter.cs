using LinkUp.Application.Abstractions.Services;
using LinkUp.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LinkUp.Web.Filters;

// Action Filter que inyecta automáticamente el conteo de solicitudes pendientes 
// en el ViewBag para todos los controladores autenticados
public class PendingRequestsFilter : IAsyncActionFilter
{
    private readonly IFriendRequestService _requestService;
    private readonly UserManager<AppUser> _userManager;

    public PendingRequestsFilter(IFriendRequestService requestService, UserManager<AppUser> userManager)
    {
        _requestService = requestService;
        _userManager = userManager;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Solo inyectar si el usuario está autenticado
        if (context.HttpContext.User.Identity?.IsAuthenticated == true)
        {
            try
            {
                var userId = int.Parse(_userManager.GetUserId(context.HttpContext.User)!);
                var pending = await _requestService.GetPendingRequestsAsync(userId);

                if (context.Controller is Controller controller)
                {
                    controller.ViewBag.PendingRequestsCount = pending.Count();
                }
            }
            catch
            {
                // Si falla, simplemente no muestra el badge
                if (context.Controller is Controller controller)
                {
                    controller.ViewBag.PendingRequestsCount = 0;
                }
            }
        }

        await next();
    }
}
