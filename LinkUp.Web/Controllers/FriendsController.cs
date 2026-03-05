using LinkUp.Application.Abstractions.Services;
using LinkUp.Domain.Entities;
using LinkUp.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LinkUp.Web.Controllers;

[Authorize]
public class FriendsController : Controller
{
    private readonly IFriendService _friendService;
    private readonly IPostService _postService;
    private readonly ICommentService _commentService;
    private readonly IAccountService _accountService;
    private readonly UserManager<AppUser> _userManager;

    public FriendsController(IFriendService friendService, IPostService postService,
        ICommentService commentService, IAccountService accountService,
        UserManager<AppUser> userManager)
    {
        _friendService = friendService;
        _postService = postService;
        _commentService = commentService;
        _accountService = accountService;
        _userManager = userManager;
    }

    private int GetUserId() => int.Parse(_userManager.GetUserId(User)!);

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = GetUserId();
        var friends = await _friendService.GetFriendsAsync(userId);
        var friendPosts = await _friendService.GetFriendPostsAsync(userId);
        var currentUser = await _accountService.GetUserByIdAsync(userId);

        var vm = new FriendsIndexViewModel
        {
            FriendPosts = friendPosts,
            Friends = friends,
            CurrentUserId = userId,
            CurrentUserProfilePicture = currentUser?.ProfilePicture
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Publications(int friendId)
    {
        var userId = GetUserId();
        var posts = await _friendService.GetFriendPublicationsAsync(userId, friendId);
        var friend = await _userManager.FindByIdAsync(friendId.ToString());

        var vm = new FriendPublicationsViewModel
        {
            Posts = posts,
            FriendId = friendId,
            FriendUserName = friend?.UserName ?? "Desconocido",
            FriendProfilePicture = friend?.ProfilePicture,
            CurrentUserId = userId
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmRemove(int friendId)
    {
        var friend = await _userManager.FindByIdAsync(friendId.ToString());
        if (friend == null) return NotFound();

        var vm = new ConfirmRemoveFriendViewModel
        {
            FriendId = friendId,
            FriendUserName = friend.UserName ?? "Desconocido",
            FriendProfilePicture = friend.ProfilePicture
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int friendId)
    {
        var userId = GetUserId();
        var result = await _friendService.RemoveFriendAsync(userId, friendId);

        if (!result.IsSuccess)
            TempData["Error"] = result.Error;
        else
            TempData["Success"] = "Amigo eliminado correctamente.";

        return RedirectToAction("Index");
    }
}
