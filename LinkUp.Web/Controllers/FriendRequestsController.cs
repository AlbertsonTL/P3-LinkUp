using LinkUp.Application.Abstractions.Services;
using LinkUp.Domain.Entities;
using LinkUp.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LinkUp.Web.Controllers;

[Authorize]
public class FriendRequestsController : Controller
{
    private readonly IFriendRequestService _requestService;
    private readonly UserManager<AppUser> _userManager;

    public FriendRequestsController(IFriendRequestService requestService, UserManager<AppUser> userManager)
    {
        _requestService = requestService;
        _userManager = userManager;
    }

    private int GetUserId() => int.Parse(_userManager.GetUserId(User)!);

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = GetUserId();
        var vm = new FriendRequestsIndexViewModel
        {
            PendingRequests = await _requestService.GetPendingRequestsAsync(userId),
            SentRequests = await _requestService.GetSentRequestsAsync(userId)
        };
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> NewRequest(string? search)
    {
        var userId = GetUserId();
        var users = await _requestService.GetUsersForNewRequestAsync(userId, search);

        var vm = new NewFriendRequestViewModel
        {
            AvailableUsers = users,
            Search = search
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(NewFriendRequestViewModel model)
    {
        var userId = GetUserId();

        if (model.SelectedUserId == null)
        {
            TempData["Error"] = "Debe seleccionar un usuario de la lista.";
            return RedirectToAction("NewRequest");
        }

        var result = await _requestService.SendRequestAsync(userId, model.SelectedUserId.Value);

        if (!result.IsSuccess)
            TempData["Error"] = result.Error;
        else
            TempData["Success"] = "Solicitud de amistad enviada correctamente.";

        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmAccept(int requestId)
    {
        var userId = GetUserId();
        var requests = await _requestService.GetPendingRequestsAsync(userId);
        var req = requests.FirstOrDefault(r => r.Id == requestId);
        if (req == null) return NotFound();

        var vm = new ConfirmAcceptRequestViewModel
        {
            RequestId = requestId,
            SenderUserName = req.SenderUserName,
            SenderProfilePicture = req.SenderProfilePicture
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Accept(int requestId)
    {
        var userId = GetUserId();
        var result = await _requestService.AcceptRequestAsync(requestId, userId);

        if (!result.IsSuccess)
            TempData["Error"] = result.Error;
        else
            TempData["Success"] = "Solicitud aceptada. ¡Ahora son amigos!";

        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmReject(int requestId)
    {
        var userId = GetUserId();
        var requests = await _requestService.GetPendingRequestsAsync(userId);
        var req = requests.FirstOrDefault(r => r.Id == requestId);
        if (req == null) return NotFound();

        var vm = new ConfirmRejectRequestViewModel
        {
            RequestId = requestId,
            SenderUserName = req.SenderUserName,
            SenderProfilePicture = req.SenderProfilePicture
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int requestId)
    {
        var userId = GetUserId();
        var result = await _requestService.RejectRequestAsync(requestId, userId);

        if (!result.IsSuccess) TempData["Error"] = result.Error;
        else TempData["Success"] = "Solicitud rechazada.";

        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmDelete(int requestId)
    {
        var userId = GetUserId();
        var requests = await _requestService.GetSentRequestsAsync(userId);
        var req = requests.FirstOrDefault(r => r.Id == requestId);
        if (req == null) return NotFound();

        var vm = new ConfirmDeleteRequestViewModel
        {
            RequestId = requestId,
            ReceiverUserName = req.ReceiverUserName,
            ReceiverProfilePicture = req.ReceiverProfilePicture
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRequest(int requestId)
    {
        var userId = GetUserId();
        var result = await _requestService.DeleteRequestAsync(requestId, userId);

        if (!result.IsSuccess) TempData["Error"] = result.Error;
        else TempData["Success"] = "Solicitud eliminada.";

        return RedirectToAction("Index");
    }
}
