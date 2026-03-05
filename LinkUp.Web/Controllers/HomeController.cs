using Microsoft.AspNetCore.Mvc;

namespace LinkUp.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Posts");
        return RedirectToAction("Login", "Account");
    }

    [Route("Home/Error")]
    public IActionResult Error() => View();
}
