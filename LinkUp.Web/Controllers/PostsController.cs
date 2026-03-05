using LinkUp.Application.Abstractions.Services;
using LinkUp.Application.DTOs.Request;
using LinkUp.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using LinkUp.Domain.Entities;

namespace LinkUp.Web.Controllers;

[Authorize]
public class PostsController : Controller
{
    private readonly IPostService _postService;
    private readonly ICommentService _commentService;
    private readonly IAccountService _accountService;
    private readonly UserManager<AppUser> _userManager;

    public PostsController(IPostService postService, ICommentService commentService,
        IAccountService accountService, UserManager<AppUser> userManager)
    {
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
        var posts = await _postService.GetUserPostsAsync(userId, userId);
        var user = await _accountService.GetUserByIdAsync(userId);

        return View(new HomeViewModel
        {
            Posts = posts,
            CurrentUserId = userId,
            CurrentUserProfilePicture = user?.ProfilePicture
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePostViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Por favor completa todos los campos requeridos.";
            return RedirectToAction("Index");
        }

        var userId = GetUserId();

        // Convert IFormFile → byte[] in Web layer
        byte[]? imageData = null;
        string? imageFileName = null;
        if (model.ImageFile != null && model.ImageFile.Length > 0)
        {
            using var ms = new MemoryStream();
            await model.ImageFile.CopyToAsync(ms);
            imageData = ms.ToArray();
            imageFileName = model.ImageFile.FileName;
        }

        var result = await _postService.CreatePostAsync(userId, new CreatePostRequestDto
        {
            Content = model.Content,
            MediaTypeStr = model.MediaType,
            ImageData = imageData,
            ImageFileName = imageFileName,
            YouTubeUrl = model.YouTubeUrl
        });

        if (!result.IsSuccess)
            TempData["Error"] = result.Error;

        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var userId = GetUserId();
        var posts = await _postService.GetUserPostsAsync(userId, userId);
        var post = posts.FirstOrDefault(p => p.Id == id);

        if (post == null || post.UserId != userId) return Forbid();

        return View(new EditPostViewModel
        {
            Id = post.Id,
            Content = post.Content,
            MediaType = post.MediaType,
            YouTubeUrl = post.YouTubeUrl,
            CurrentImagePath = post.ImagePath
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditPostViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var userId = GetUserId();

        byte[]? imageData = null;
        string? imageFileName = null;
        if (model.ImageFile != null && model.ImageFile.Length > 0)
        {
            using var ms = new MemoryStream();
            await model.ImageFile.CopyToAsync(ms);
            imageData = ms.ToArray();
            imageFileName = model.ImageFile.FileName;
        }

        var result = await _postService.UpdatePostAsync(model.Id, userId, new UpdatePostRequestDto
        {
            Content = model.Content,
            YouTubeUrl = model.YouTubeUrl,
            ImageData = imageData,
            ImageFileName = imageFileName
        });

        if (!result.IsSuccess)
        {
            ModelState.AddModelError("", result.Error!);
            return View(model);
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();
        await _postService.DeletePostAsync(id, userId);
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> React(int postId, bool isLike, string returnTo = "Index")
    {
        var userId = GetUserId();
        await _postService.ReactToPostAsync(postId, userId, isLike);

        return returnTo == "Friends"
            ? RedirectToAction("Index", "Friends")
            : RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(AddCommentViewModel model, string returnTo = "Index")
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "El comentario no puede estar vacío.";
            return returnTo == "Friends"
                ? RedirectToAction("Index", "Friends")
                : RedirectToAction("Index");
        }

        var userId = GetUserId();
        await _commentService.AddCommentAsync(model.PostId, userId, model.Content, model.ParentCommentId);

        return returnTo == "Friends"
            ? RedirectToAction("Index", "Friends")
            : RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteComment(int commentId, string returnTo = "Index")
    {
        var userId = GetUserId();
        await _commentService.DeleteCommentAsync(commentId, userId);

        return returnTo == "Friends"
            ? RedirectToAction("Index", "Friends")
            : RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> EditComment(int commentId, string returnTo = "Index")
    {
        var userId = GetUserId();
        // Fetch the comment via comment service (we use a simple approach)
        // The comment must belong to the user
        var posts = await _postService.GetUserPostsAsync(userId, userId);
        var allComments = posts
            .SelectMany(p => p.Comments)
            .Concat(posts.SelectMany(p => p.Comments.SelectMany(c => c.Replies)))
            .FirstOrDefault(c => c.Id == commentId && c.UserId == userId);

        if (allComments == null)
        {
            // Try friend posts
            var friendPosts = await _postService.GetFriendPostsAsync(userId);
            allComments = friendPosts
                .SelectMany(p => p.Comments)
                .Concat(friendPosts.SelectMany(p => p.Comments.SelectMany(c => c.Replies)))
                .FirstOrDefault(c => c.Id == commentId && c.UserId == userId);
        }

        if (allComments == null) return Forbid();

        return View(new EditCommentViewModel
        {
            CommentId = commentId,
            PostId = allComments.PostId,
            Content = allComments.Content,
            ReturnTo = returnTo
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditComment(EditCommentViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var userId = GetUserId();
        var result = await _commentService.UpdateCommentAsync(model.CommentId, userId, model.Content);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError("", result.Error!);
            return View(model);
        }

        return model.ReturnTo == "Friends"
            ? RedirectToAction("Index", "Friends")
            : RedirectToAction("Index");
    }
}
